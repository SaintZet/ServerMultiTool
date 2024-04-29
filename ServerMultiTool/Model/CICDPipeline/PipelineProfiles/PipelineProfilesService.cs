using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using log4net;
using ServerMultiTool.Model.Settings;

namespace ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

public static class PipelineProfilesService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PipelineProfilesService));

    private const string SettingsFolderName = @"AppSettings\Profiles";
    private const string SearchPattern = "*.json";

    public static PipelineProfile[] PipelineProfiles { get; private set; } = null!;

    public static void LoadOrInitialize(string appDirectory)
    {
        var pathToFolder = Path.Combine(appDirectory, SettingsFolderName);

        var profiles = TryLoadSettingsFrom(pathToFolder);
        if (profiles.Any())
        {
            Log.Info($"{nameof(PipelineProfiles)} have been successfully loaded.");
        }
        else
        {
            profiles = InitializeDefaultProfiles(pathToFolder);
            Log.Info($"{nameof(PipelineProfiles)} have been successfully initialized.");
        }

        PipelineProfiles = profiles;
    }

    private static PipelineProfile[] InitializeDefaultProfiles(string pathToFolder)
    {
        var appSettings = AppSettingsService.AppSettings;
        var solutionDir = appSettings.SolutionDirectories[0];
        var httpDir = appSettings.HttpDirectories[0];
        
        var standardProfile = DefaultValues.GetStandardProfile();
        SaveSettingsTo(standardProfile, Path.Combine(pathToFolder, $"{standardProfile.Name}.json"));
        
        var extendedProfile = DefaultValues.GetExtendedProfile(solutionDir, httpDir);
        SaveSettingsTo(extendedProfile, Path.Combine(pathToFolder, $"{extendedProfile.Name}.json"));
        
        return new[] { standardProfile, extendedProfile };
    }

    private static PipelineProfile[] TryLoadSettingsFrom(string pathToFolder)
    {
        if (Directory.Exists(pathToFolder) is false)
            return Array.Empty<PipelineProfile>();
        
        return Directory.GetFiles(pathToFolder, SearchPattern)
            .Select(filePath =>
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true,
                };
        
                return JsonSerializer.Deserialize<PipelineProfile>(json, options);
            })
            .ToArray();
    }

    public static void SaveSettingsAsync()
    {
        throw new NotImplementedException();
    }

    public static void UpdateSetting(string key, object value)
    {
        throw new NotImplementedException();
    }

    private static void SaveSettingsTo(PipelineProfile profiles, string path)
    {
        var directoryPath = Path.GetDirectoryName(path);
        if (directoryPath is null)
            throw new Exception(); //TODO
        
        Directory.CreateDirectory(directoryPath);
        
        var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllTextAsync(path, json);
    }
}