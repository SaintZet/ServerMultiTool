using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using log4net;
using ServerMultiTool.Model.Common.DefaultValues;
using ServerMultiTool.Model.Settings;

namespace ServerMultiTool.Model.Pipeline.Profiles;

public static class PipelineProfilesService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PipelineProfilesService));

    private const string SettingsFolderName = @"AppSettings\Profiles";
    private const string SearchPattern = "*.json";

    private static readonly List<PipelineProfile> _pipelineProfiles = [];
    public static PipelineProfile[] PipelineProfiles => _pipelineProfiles.ToArray();

    public static void LoadOrInitialize(string appDirectory)
    {
        var pathToFolder = Path.Combine(appDirectory, SettingsFolderName);

        var profiles = TryLoadSettingsFrom(pathToFolder);
        if (profiles.Length is not 0)
        {
            Log.Info($"{nameof(PipelineProfiles)} have been successfully loaded.");
        }
        else
        {
            profiles = InitializeDefaultProfiles(pathToFolder);
            Log.Info($"{nameof(PipelineProfiles)} have been successfully initialized.");
        }

        _pipelineProfiles.AddRange(profiles);
    }

    private static PipelineProfile[] InitializeDefaultProfiles(string pathToFolder)
    {
        var appSettings = AppSettingsService.AppSettings;
        var solutionDir = appSettings.SolutionDirectories.FirstOrDefault();
        var httpDir = appSettings.HttpDirectories.FirstOrDefault();

        var devProfile = DefaultProfiles.GetDevProfile();
        SaveSettingsTo(devProfile, Path.Combine(pathToFolder, $"{devProfile.Name}.json"));
        
        var standardProfile = DefaultProfiles.GetStandardProfile();
        SaveSettingsTo(standardProfile, Path.Combine(pathToFolder, $"{standardProfile.Name}.json"));
        
        var extendedProfile = DefaultProfiles.GetExtendedProfile(solutionDir, httpDir);
        SaveSettingsTo(extendedProfile, Path.Combine(pathToFolder, $"{extendedProfile.Name}.json"));

        return [devProfile, standardProfile, extendedProfile];
    }

    private static PipelineProfile[] TryLoadSettingsFrom(string pathToFolder)
    {
        if (!Directory.Exists(pathToFolder))
            return [];
        
        return Directory.GetFiles(pathToFolder, SearchPattern)
            .Select(filePath =>
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true
                };
        
                return JsonSerializer.Deserialize<PipelineProfile>(json, options);
            }).ToArray();
    }

    public static void AddProfile(PipelineProfile profile)
    {
        if (_pipelineProfiles.Any(x => x.Name == profile.Name))
            return;

        _pipelineProfiles.Add(profile);
        SaveProfiles();
    }

    public static void RemoveProfile(PipelineProfile profile)
    {
        if (_pipelineProfiles.Remove(profile))
            SaveProfiles();
    }

    public static void UpdateProfile(PipelineProfile oldProfile, PipelineProfile newProfile)
    {
        var index = _pipelineProfiles.IndexOf(oldProfile);
        if (index == -1)
            return;

        _pipelineProfiles[index] = newProfile;
        SaveProfiles();
    }

    public static void SavePipelineProfiles(IEnumerable<PipelineProfile> profiles)
    {
        var pathToFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFolderName);
        
        if (!Directory.Exists(pathToFolder))
            Directory.CreateDirectory(pathToFolder);
        
        foreach (var profile in profiles)
        {
            var path = Path.Combine(pathToFolder, $"{profile.Name}.json");
            SaveSettingsTo(profile, path);
        }
        
        Log.Info($"{nameof(PipelineProfiles)} have been successfully saved.");
    }

    public static void UpdateSetting(string key, object value)
    {
        throw new NotImplementedException();
    }

    private static void SaveSettingsTo(PipelineProfile profile, string path)
    {
        var directoryPath = Path.GetDirectoryName(path);
        if (directoryPath is null)
            throw new Exception("Directory path is null.");

        Directory.CreateDirectory(directoryPath);

        var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static void SaveProfiles()
    {
        var settings = AppSettingsService.AppSettings;
        // Добавить логику сохранения профилей
        AppSettingsService.SaveAppSettings(settings);
    }
}