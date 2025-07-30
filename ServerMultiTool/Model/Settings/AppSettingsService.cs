using log4net;
using log4net.Config;
using ServerMultiTool.Model.Common.DefaultValues;
using System;
using System.IO;
using System.Text.Json;

namespace ServerMultiTool.Model.Settings
{
    public static class AppSettingsService
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(AppSettingsService));
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        private const string SettingsFolderName = "AppSettings";
        private const string SettingsFileName = "AppSettings.json";

        private static string PathToSettingFile = null!;
        private static string AppSettingsDirectory = null!;

        public static AppSettings AppSettings { get; private set; }

        public static void SaveAppSettings(AppSettings settings)
        {
            if (string.IsNullOrEmpty(PathToSettingFile))
                throw new InvalidOperationException("Settings file path is not initialized.");

            SaveSettingsTo(settings, PathToSettingFile);
            Log.Info($"{nameof(AppSettings)} have been successfully saved.");
        }

        public static AppSettings LoadOrInitialize() =>
            LoadOrInitialize(AppSettingsDirectory);

        public static AppSettings LoadOrInitialize(string appSettingsDirectory)
        {
            AppSettingsDirectory = appSettingsDirectory;

            var pathToFolder = Path.Combine(appSettingsDirectory, SettingsFolderName);

            if (Directory.Exists(pathToFolder) is false)
                Directory.CreateDirectory(pathToFolder);

            PathToSettingFile = Path.Combine(pathToFolder, SettingsFileName);

            AppSettings = File.Exists(PathToSettingFile) ? LoadSettingsFrom(PathToSettingFile) : InitializeDefaultSettings(PathToSettingFile);

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
            var directoryPath = Path.GetDirectoryName(path)
                ?? throw new Exception(); // todo: add message

            Directory.CreateDirectory(directoryPath);

            var json = JsonSerializer.Serialize(settings, JsonSerializerOptions);
            File.WriteAllTextAsync(path, json);
        }
    }
}