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
    
    private static readonly JsonSerializerOptions ReadJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
    };
    
    private static readonly JsonSerializerOptions WriteJsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

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

        _pipelineProfiles.AddRange(profiles!);
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

    private static PipelineProfile?[] TryLoadSettingsFrom(string pathToFolder)
    {
        if (!Directory.Exists(pathToFolder))
        {
            Log.Info($"Profiles directory {pathToFolder} does not exist.");
            return [];
        }
    
        var files = Directory.GetFiles(pathToFolder, SearchPattern);
        Log.Info($"Found {files.Length} profile files in {pathToFolder}.");
    
        var profiles = new List<PipelineProfile?>();
    
        foreach (var filePath in files)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var profile = JsonSerializer.Deserialize<PipelineProfile>(json, ReadJsonSerializerOptions);
                profiles.Add(profile);
                Log.Info($"Successfully loaded profile from {Path.GetFileName(filePath)}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load profile from {Path.GetFileName(filePath)}: {ex.Message}");
                profiles.Add(null);
            }
        }
    
        return profiles.ToArray();
}

    public static void SavePipelineProfiles(IEnumerable<PipelineProfile> profiles)
    {
        var pathToFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFolderName);
        
        if (!Directory.Exists(pathToFolder))
            Directory.CreateDirectory(pathToFolder);

        var profilesList = profiles.ToList();
        var profileNames = profilesList.Select(p => $"{p.Name}.json").ToHashSet();
        
        var existingFiles = Directory.GetFiles(pathToFolder, SearchPattern);
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
            var directoryPath = Path.GetDirectoryName(path);
            if (directoryPath is null)
                throw new Exception("Directory path is null.");

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