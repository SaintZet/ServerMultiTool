using System;
using System.IO;
using System.Text.Json;
using log4net;
using log4net.Config;
using ServerMultiTool.Model.Common.DefaultValues;

namespace ServerMultiTool.Model.Settings
{
    public static class AppSettingsService
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(AppSettingsService));

        private const string SettingsFolderName = "AppSettings";
        private const string SettingsFileName = "AppSettings.json";
        
        private static string _pathToSettingFile = null!;
        private static string _appSettingsDirectory = null!;

        public static AppSettings AppSettings { get; private set; }

        public static void UpdateSettings(string key, object value)
        {
            throw new NotImplementedException();

            SaveSettingsTo(AppSettings, _pathToSettingFile);
        }
        
        public static void SaveAppSettings(AppSettings settings)
        {
            if (string.IsNullOrEmpty(_pathToSettingFile))
                throw new InvalidOperationException("Settings file path is not initialized.");

            SaveSettingsTo(settings, _pathToSettingFile);
            Log.Info($"{nameof(AppSettings)} have been successfully saved.");
        }

        public static AppSettings LoadOrInitialize() => 
            LoadOrInitialize(_appSettingsDirectory);

        public static AppSettings LoadOrInitialize(string appSettingsDirectory)
        {
            _appSettingsDirectory = appSettingsDirectory;
            
            var pathToFolder = Path.Combine(appSettingsDirectory, SettingsFolderName);
            if (Directory.Exists(pathToFolder) is false)
                Directory.CreateDirectory(pathToFolder);
            
            var pathToFile = Path.Combine(pathToFolder, SettingsFileName);
            _pathToSettingFile = pathToFile;
            
            AppSettings = File.Exists(pathToFile) ? LoadSettingsFrom(pathToFile) : InitializeDefaultSettings(pathToFile);
            
            var logConfig = new FileInfo(AppSettings.Log4NetConfigPath);
            XmlConfigurator.Configure(logConfig);
            
            Log.Info($"{nameof(AppSettings)} have been successfully loaded.");
            
            return AppSettings;
        }

        private static AppSettings LoadSettingsFrom(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json);
        }

        private static AppSettings InitializeDefaultSettings(string path)
        {
            var defaultSettings = DefaultAppSettings.GetDefaultAppSettings();
            
            SaveSettingsTo(defaultSettings, path);

            return defaultSettings;
        }

        private static void SaveSettingsTo(AppSettings settings, string path)
        {
            var directoryPath = Path.GetDirectoryName(path);
            if (directoryPath is null)
                throw new Exception(); //TODO
            
            Directory.CreateDirectory(directoryPath);
            
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllTextAsync(path, json);
        }
    }
}