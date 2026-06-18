using ServerMultiTool.Model.Features.Pipeline.Profile;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Model.Infrastructure.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ServerMultiTool.Model.Infrastructure.Services
{
    public sealed class FilePipelineProfilesService : IPipelineProfilesService, IDisposable
    {
        private readonly string _dir;
        private readonly JsonSerializerOptions _json;
        private readonly object _lock = new();
        private readonly Dictionary<Guid, PipelineProfile> _cache = new();
        private readonly Dictionary<Guid, string> _fileIndex = new(); // Id -> file path
        private FileSystemWatcher? _fsw;

        public event EventHandler? ProfilesChanged;

        public FilePipelineProfilesService(string directory,
                                           JsonSerializerOptions? jsonOptions = null,
                                           bool watchForExternalChanges = true)
        {
            _dir = directory;
            Directory.CreateDirectory(_dir);
            _json = jsonOptions ?? GetDefaultOptions();

            LoadAllIntoCache();

            if (watchForExternalChanges)
            {
                _fsw = new FileSystemWatcher(_dir, "*.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
                };
                _fsw.Changed += (_, __) => ReloadOnFsEvent();
                _fsw.Created += (_, __) => ReloadOnFsEvent();
                _fsw.Deleted += (_, __) => ReloadOnFsEvent();
                _fsw.Renamed += (_, __) => ReloadOnFsEvent();
                _fsw.EnableRaisingEvents = true;
            }
        }

        public IReadOnlyList<PipelineProfile> GetAll()
        {
            lock (_lock)
            {
                return _cache.Values.Select(Clone).OrderBy(p => p.Name).ToList();
            }
        }

        public PipelineProfile? GetById(Guid id)
        {
            lock (_lock)
            {
                return _cache.TryGetValue(id, out var p) ? Clone(p) : null;
            }
        }

        public PipelineProfile? GetByName(string name)
        {
            lock (_lock)
            {
                var p = _cache.Values.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                return p is null ? null : Clone(p);
            }
        }

        public PipelineProfile Add(PipelineProfile profile)
        {
            if (profile is null) throw new ArgumentNullException(nameof(profile));

            lock (_lock)
            {
                var copy = Clone(profile);

                // имя файла по безопасному имени
                var filePath = Path.Combine(_dir, MakeSafeFileName(copy.Name) + ".json");
                // если занято — добавляем суффикс
                filePath = EnsureUniquePath(filePath);

                WriteToFile(filePath, copy);

                _cache[copy.Id] = copy;
                _fileIndex[copy.Id] = filePath;

                RaiseChangedUnlocked();
                return Clone(copy);
            }
        }

        public void Update(PipelineProfile profile)
        {
            if (profile is null || profile.Id == Guid.Empty)
                throw new ArgumentException("Profile must have Id");

            lock (_lock)
            {
                if (!_fileIndex.TryGetValue(profile.Id, out var path))
                    throw new KeyNotFoundException("Profile not found");

                var old = _cache[profile.Id];
                var renamed = !string.Equals(old.Name, profile.Name, StringComparison.Ordinal);

                // если имя поменялось — возможно переименуем файл
                if (renamed)
                {
                    var newPath = Path.Combine(_dir, MakeSafeFileName(profile.Name) + ".json");
                    newPath = EnsureUniquePath(newPath, except: path);

                    // пишем в новый, затем удаляем старый (чтобы не потерять данные в случае сбоя)
                    WriteToFile(newPath, profile);
                    TryDeleteFile(path);
                    path = newPath;
                    _fileIndex[profile.Id] = path;
                }
                else
                {
                    WriteToFile(path, profile);
                }

                _cache[profile.Id] = Clone(profile);
                RaiseChangedUnlocked();
            }
        }

        public void UpdateField<T>(Guid profileId, string fieldPath, T value)
        {
            if (profileId == Guid.Empty) throw new ArgumentException(nameof(profileId));
            if (string.IsNullOrWhiteSpace(fieldPath)) throw new ArgumentException(nameof(fieldPath));

            lock (_lock)
            {
                if (!_cache.TryGetValue(profileId, out var p) || !_fileIndex.TryGetValue(profileId, out var path))
                    throw new KeyNotFoundException("Profile not found");

                // простой рефлекшн по "A.B.C" (безопасно для ваших POCO)
                ApplyFieldPath(p, fieldPath, value);

                WriteToFile(path, p);
                _cache[profileId] = Clone(p);
                RaiseChangedUnlocked();
            }
        }

        public bool Delete(Guid profileId)
        {
            if (profileId == Guid.Empty) return false;

            lock (_lock)
            {
                var existed = _cache.Remove(profileId);
                if (_fileIndex.Remove(profileId, out var path))
                    TryDeleteFile(path);

                if (existed) RaiseChangedUnlocked();
                return existed;
            }
        }

        public void SaveAll(IEnumerable<PipelineProfile> profiles)
        {
            if (profiles is null) throw new ArgumentNullException(nameof(profiles));

            lock (_lock)
            {
                // простая стратегия: переписать все переданные профили по их Id/Name
                var incoming = profiles.ToList();

                // 1) записать/обновить входящие
                foreach (var p in incoming)
                {
                    var name = string.IsNullOrWhiteSpace(p.Name) ? $"Profile-{p.Id:N}" : p.Name;

                    if (!_fileIndex.TryGetValue(p.Id, out var existingPath))
                    {
                        var newPath = EnsureUniquePath(Path.Combine(_dir, MakeSafeFileName(name) + ".json"));
                        WriteToFile(newPath, p);
                        _fileIndex[p.Id] = newPath;
                    }
                    else
                    {
                        // переименование файла, если имя поменялось
                        var desiredPath = Path.Combine(_dir, MakeSafeFileName(name) + ".json");
                        desiredPath = EnsureUniquePath(desiredPath, except: existingPath);

                        if (!Path.GetFullPath(existingPath).Equals(Path.GetFullPath(desiredPath), StringComparison.OrdinalIgnoreCase))
                        {
                            WriteToFile(desiredPath, p);
                            TryDeleteFile(existingPath);
                            _fileIndex[p.Id] = desiredPath;
                        }
                        else
                        {
                            WriteToFile(existingPath, p);
                        }
                    }

                    _cache[p.Id] = Clone(p);
                }

                // 2) удалить то, чего нет во входящих
                var incomingIds = new HashSet<Guid>(incoming.Select(x => x.Id));
                var toDelete = _cache.Keys.Where(id => !incomingIds.Contains(id)).ToList();
                foreach (var id in toDelete)
                    Delete(id);

                RaiseChangedUnlocked();
            }
        }

        // ===== helpers =====

        private void LoadAllIntoCache()
        {
            lock (_lock)
            {
                _cache.Clear();
                _fileIndex.Clear();

                foreach (var file in Directory.EnumerateFiles(_dir, "*.json"))
                {
                    try
                    {
                        using var fs = File.OpenRead(file);
                        var p = JsonSerializer.Deserialize<PipelineProfile>(fs, _json);

                        if (p is null)
                            continue;

                        _cache[p.Id] = p;
                        _fileIndex[p.Id] = file;
                    }
                    catch
                    {
                        // логирование по месту (log4net/ILogger), но не падаем из‑за одного файла
                    }
                }
            }
        }

        private void ReloadOnFsEvent()
        {
            // дебаунс можно добавить при желании
            LoadAllIntoCache();
            RaiseChanged();
        }

        private void WriteToFile(string path, PipelineProfile profile)
        {
            // атомарная запись: в temp, затем replace
            var tmp = path + ".tmp";
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using (var fs = File.Create(tmp))
                JsonSerializer.Serialize(fs, profile, _json);
            File.Copy(tmp, path, true);
            File.Delete(tmp);
        }

        private static void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { /* лог */ }
        }

        private static string MakeSafeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);
            foreach (var ch in name)
                sb.Append(invalid.Contains(ch) ? '_' : ch);
            var s = sb.ToString().Trim();
            return string.IsNullOrEmpty(s) ? "profile" : s;
        }

        private string EnsureUniquePath(string desired, string? except = null)
        {
            if (!File.Exists(desired) || string.Equals(desired, except, StringComparison.OrdinalIgnoreCase))
                return desired;

            var dir = Path.GetDirectoryName(desired)!;
            var baseName = Path.GetFileNameWithoutExtension(desired);
            var ext = Path.GetExtension(desired);
            for (int i = 2; ; i++)
            {
                var candidate = Path.Combine(dir, $"{baseName} ({i}){ext}");
                if (!File.Exists(candidate) || string.Equals(candidate, except, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }
        }

        private static void ApplyFieldPath<T>(object target, string path, T value)
        {
            var parts = path.Split('.');
            object? current = target;
            var type = current!.GetType();

            for (int i = 0; i < parts.Length; i++)
            {
                var prop = type.GetProperty(parts[i]);
                if (prop == null) throw new ArgumentException($"Property '{parts[i]}' not found in {type.Name}");

                if (i == parts.Length - 1)
                {
                    if (!prop.CanWrite) throw new InvalidOperationException($"Property '{prop.Name}' not writable");
                    prop.SetValue(current, value);
                }
                else
                {
                    current = prop.GetValue(current);
                    if (current == null)
                    {
                        // если промежуточное звено null — создаём инстанс
                        var newObj = Activator.CreateInstance(prop.PropertyType)
                                   ?? throw new InvalidOperationException($"Cannot create instance of {prop.PropertyType.Name}");
                        prop.SetValue(target, newObj); // привязка к родителю
                        current = newObj;
                    }
                    type = current.GetType();
                }
            }
        }

        private PipelineProfile Clone(PipelineProfile p)
        {
            // быстрый глубокий клон через JSON
            var json = JsonSerializer.Serialize(p, _json);
            return JsonSerializer.Deserialize<PipelineProfile>(json, _json)!;
        }

        private static JsonSerializerOptions GetDefaultOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
                WriteIndented = true
            };
            options.Converters.Add(new PipelineOperationJsonConverter());
            return options;
        }

        private void RaiseChanged()
        {
            try { ProfilesChanged?.Invoke(this, EventArgs.Empty); } catch { /* игнор */ }
        }
        private void RaiseChangedUnlocked() => RaiseChanged();

        public void Dispose()
        {
            if (_fsw is not null)
            {
                _fsw.EnableRaisingEvents = false;
                _fsw.Dispose();
                _fsw = null;
            }
        }
    }
}
