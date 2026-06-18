using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Infrastructure.Services
{
    public class FileAppSettingsService : IAppSettingsService
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly object _lock = new();
        private AppSettings? _cached;

        public event EventHandler<AppSettings>? Changed;

        public FileAppSettingsService(string filePath, JsonSerializerOptions? serializerOptions = null)
        {
            _filePath = filePath;
            _serializerOptions = serializerOptions ?? GetDefaultOptions();

            // загружаем в кэш при старте (если файл есть)
            if (File.Exists(_filePath))
                _cached = LoadFromFile();
            else
                _cached = new AppSettings();
        }

        public AppSettings Get()
        {
            lock (_lock)
            {
                if (_cached == null)
                    _cached = LoadFromFile();
                return Clone(_cached);
            }
        }

        public async Task<AppSettings> GetAsync()
        {
            lock (_lock)
            {
                if (_cached != null)
                    return Clone(_cached);
            }

            // асинхронное чтение
            using var fs = File.OpenRead(_filePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(fs, _serializerOptions)
                ?? new AppSettings();

            lock (_lock)
            {
                _cached = settings;
                return Clone(_cached);
            }
        }

        public void Save(AppSettings settings)
        {
            lock (_lock)
            {
                SaveToFile(settings);
                _cached = Clone(settings);
            }
            Changed?.Invoke(this, Clone(settings));
        }

        public async Task SaveAsync(AppSettings settings)
        {
            lock (_lock)
            {
                _cached = Clone(settings);
            }

            await using var fs = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(fs, settings, _serializerOptions);

            Changed?.Invoke(this, Clone(settings));
        }

        public void UpdateField<T>(string fieldName, T value)
        {
            lock (_lock)
            {
                if (_cached == null)
                    _cached = LoadFromFile();

                var prop = typeof(AppSettings).GetProperty(fieldName);
                if (prop == null || !prop.CanWrite)
                    throw new ArgumentException($"Property {fieldName} not found or not writable");

                prop.SetValue(_cached, value);
                SaveToFile(_cached);
            }
            Changed?.Invoke(this, Get());
        }
        private AppSettings LoadFromFile()
        {
            if (!File.Exists(_filePath))
                return new AppSettings();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json, _serializerOptions)
                   ?? new AppSettings();
        }

        private void SaveToFile(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, _serializerOptions);
            File.WriteAllText(_filePath, json);
        }

        private AppSettings Clone(AppSettings source)
        {
            // глубокое копирование через сериализацию
            var json = JsonSerializer.Serialize(source, _serializerOptions);
            return JsonSerializer.Deserialize<AppSettings>(json, _serializerOptions)
                   ?? new AppSettings();
        }

        private static JsonSerializerOptions GetDefaultOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true
                // можно добавить твои конвертеры, если они были в JsonRepository
            };
        }
    }

}