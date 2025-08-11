using log4net;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Infrastructure.DefaultValues;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Model.Infrastructure.Json;
using System;
using System.IO;
using System.Text.Json;

namespace ServerMultiTool.Model.Infrastructure.Repositories;

public class AppSettingsRepository : JsonRepository<AppSettings>, IAppSettingsRepository
{
    private const string SettingsFolderName = "AppSettings";
    private const string SettingsFileName = "AppSettings.json";

    private readonly string _appSettingsDirectory;

    public AppSettingsRepository(string appSettingsDirectory, ILog log)
        : base(Path.Combine(appSettingsDirectory, SettingsFolderName, SettingsFileName), log)
    {
        _appSettingsDirectory = appSettingsDirectory;
    }

    public AppSettings LoadOrInitialize()
    {
        return LoadOrInitialize(_appSettingsDirectory);
    }

    public AppSettings LoadOrInitialize(string appSettingsDirectory)
    {
        try
        {
            var pathToFolder = Path.Combine(appSettingsDirectory, SettingsFolderName);

            if (!Directory.Exists(pathToFolder))
                Directory.CreateDirectory(pathToFolder);

            var pathToSettingFile = Path.Combine(pathToFolder, SettingsFileName);

            AppSettings appSettings;

            if (File.Exists(pathToSettingFile))
            {
                var json = File.ReadAllText(pathToSettingFile);
                appSettings = JsonSerializer.Deserialize<AppSettings>(json, ReadOptions)
                    ?? InitializeDefaultSettings();
            }
            else
            {
                appSettings = InitializeDefaultSettings();
            }

            Log.Info($"{nameof(AppSettings)} have been successfully loaded.");

            return appSettings;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load or initialize {nameof(AppSettings)}: {ex.Message}", ex);
            throw;
        }
    }

    private AppSettings InitializeDefaultSettings()
    {
        var defaultSettings = DefaultAppSettings.GetDefaultAppSettings();
        Update(defaultSettings);
        return defaultSettings;
    }
}