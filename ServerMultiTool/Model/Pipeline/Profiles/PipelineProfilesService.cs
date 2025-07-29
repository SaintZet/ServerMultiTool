using log4net;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common.DefaultValues;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Pipeline.Profiles;

public static class PipelineProfilesService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PipelineProfilesService));

    private const string ProfileSearchPattern = "*.json";

    private static string _pathToProfilesDirectory = null!;
    private static FileSystemWatcher? _settingsFileWatcher;
    private static CancellationTokenSource? _fileChangeDebounceCts;

    private static readonly JsonSerializerOptions ReadJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
    };

    private static readonly JsonSerializerOptions WriteJsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public static event EventHandler? PipelineProfilesChanged;
    public static List<PipelineProfile> PipelineProfiles { get; private set; } = [];

    public static void LoadOrInitialize(string appDirectory)
    {
        var path = Path.Combine(appDirectory, AppConstants.Folders.AppSettings);
        _pathToProfilesDirectory = path;

        var profiles = TryLoadSettingsFrom(path);
        if (profiles.IsNullOrEmpty())
        {
            profiles = InitializeDefaultProfiles(path);
            Log.Info($"{nameof(PipelineProfiles)} have been successfully initialized.");
        }
        else
        {
            Log.Info($"{nameof(PipelineProfiles)} have been successfully loaded.");
        }

        PipelineProfiles = profiles;

        SetupFileWatcher(path);
    }

    private static void SetupFileWatcher(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return;

            _settingsFileWatcher = new FileSystemWatcher(path)
            {
                Filter = ProfileSearchPattern,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };

            _settingsFileWatcher.Changed += OnSettingsFileChanged;
            _settingsFileWatcher.EnableRaisingEvents = true;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to set up file watcher for {path}: {ex.Message}");
        }
    }

    private static void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
    {
        _fileChangeDebounceCts?.Cancel();
        _fileChangeDebounceCts = new CancellationTokenSource();
        var token = _fileChangeDebounceCts.Token;

        Task.Delay(700, token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
                ReloadProfiles();
        }, TaskScheduler.Default);
    }

    private static void ReloadProfiles()
    {
        PipelineProfiles = TryLoadSettingsFrom(_pathToProfilesDirectory);

        if (PipelineProfiles.IsNullOrEmpty())
        {
            PipelineProfiles = InitializeDefaultProfiles(_pathToProfilesDirectory);
            Log.Info($"{nameof(PipelineProfiles)} have been successfully re-initialized.");
        }
        else
        {
            Log.Info($"{nameof(PipelineProfiles)} have been successfully reloaded.");
        }

        PipelineProfilesChanged?.Invoke(null, EventArgs.Empty);
    }

    private static List<PipelineProfile> InitializeDefaultProfiles(string pathToFolder)
    {
        var appSettings = AppSettingsService.AppSettings;
        var solutionDir = appSettings.SolutionDirectories.FirstOrDefault();
        var httpDir = appSettings.HttpDirectories.FirstOrDefault();

        var devProfile = DefaultProfiles.GetIisResetProfile();
        SaveSettingsTo(devProfile, Path.Combine(pathToFolder, $"{devProfile.Name}.json"));

        var standardProfile = DefaultProfiles.GetStandardProfile();
        SaveSettingsTo(standardProfile, Path.Combine(pathToFolder, $"{standardProfile.Name}.json"));

        if (solutionDir == null || httpDir == null)
        {
            throw new InvalidOperationException("Solution or HTTP directory is not configured properly.");
        }

        var extendedProfile = DefaultProfiles.GetExtendedProfile(solutionDir, httpDir);
        SaveSettingsTo(extendedProfile, Path.Combine(pathToFolder, $"{extendedProfile.Name}.json"));

        return [devProfile, standardProfile, extendedProfile];
    }

    private static List<PipelineProfile> TryLoadSettingsFrom(string pathToFolder)
    {
        if (!Directory.Exists(pathToFolder))
        {
            Log.Info($"Profiles directory {pathToFolder} does not exist.");
            return [];
        }

        var files = Directory.GetFiles(pathToFolder, ProfileSearchPattern);
        Log.Info($"Found {files.Length} profile files in {pathToFolder}.");

        var profiles = new List<PipelineProfile>();

        foreach (var filePath in files)
        {
            try
            {
                var json = File.ReadAllText(filePath);

                var profile = JsonSerializer.Deserialize<PipelineProfile>(json, ReadJsonSerializerOptions)
                    ?? throw new Exception("Failed to deserialize profile.");

                profiles.Add(profile);

                Log.Info($"Successfully loaded profile from {Path.GetFileName(filePath)}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load profile from {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        return profiles;
    }

    public static void SavePipelineProfiles(IEnumerable<PipelineProfile> profiles)
    {
        var pathToFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.Folders.AppSettings);

        if (!Directory.Exists(pathToFolder))
            Directory.CreateDirectory(pathToFolder);

        var profilesList = profiles.ToList();
        var profileNames = profilesList.Select(p => $"{p.Name}.json").ToHashSet();

        var existingFiles = Directory.GetFiles(pathToFolder, ProfileSearchPattern);
        foreach (var file in existingFiles)
        {
            if (profileNames.Contains(Path.GetFileName(file)))
                continue;

            File.Delete(file);
            Log.Info($"Obsolete profile file {Path.GetFileName(file)} has been deleted.");
        }

        foreach (var profile in profilesList)
        {
            var path = Path.Combine(pathToFolder, $"{profile.Name}.json");
            SaveSettingsTo(profile, path);
        }

        Log.Info($"{nameof(PipelineProfiles)} have been successfully saved.");
    }

    private static void SaveSettingsTo(PipelineProfile profile, string path)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(path)
                ?? throw new Exception("Directory path is null.");

            Directory.CreateDirectory(directoryPath);

            var json = JsonSerializer.Serialize(profile, WriteJsonSerializerOptions);
            File.WriteAllText(path, json);

            Log.Info($"Profile '{profile.Name}' has been successfully saved to {Path.GetFileName(path)}.");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to save profile '{profile.Name}' to {Path.GetFileName(path)}: {ex.Message}");
            throw;
        }
    }
}