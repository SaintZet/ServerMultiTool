using log4net;
using log4net.Config;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Model.Infrastructure.Settings;
using System;
using System.IO;

namespace ServerMultiTool.Model.Services.Settings
{
    public static class AppSettingsService
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(AppSettingsService));
        private static IAppSettingsRepository? _repository;
        private static string _appSettingsDirectory = null!;

        // Keep a static reference for backward compatibility
        public static AppSettings AppSettings { get; private set; } = default;

        // Initialize the repository
        private static IAppSettingsRepository Repository
        {
            get
            {
                if (_repository == null)
                {
                    if (string.IsNullOrEmpty(_appSettingsDirectory))
                        throw new InvalidOperationException("AppSettingsDirectory is not initialized. Call LoadOrInitialize first.");

                    _repository = new AppSettingsRepository(_appSettingsDirectory, Log);
                }
                return _repository;
            }
        }

        public static AppSettings SaveAppSettings(AppSettings settings)
        {
            Repository.Update(settings);
            AppSettings = settings; // Update static reference
            Log.Info($"{nameof(AppSettings)} have been successfully saved.");
            return settings;
        }

        public static AppSettings UpdateAppSettingsField<T>(string fieldName, T value)
        {
            Repository.UpdateField(fieldName, value);
            AppSettings = Repository.Get(); // Update static reference
            Log.Info($"{nameof(AppSettings)} field '{fieldName}' has been successfully updated.");
            return AppSettings;
        }

        public static AppSettings LoadOrInitialize() =>
            LoadOrInitialize(AppDomain.CurrentDomain.BaseDirectory);

        public static AppSettings LoadOrInitialize(string appSettingsDirectory)
        {
            _appSettingsDirectory = appSettingsDirectory;
            var settings = Repository.LoadOrInitialize(appSettingsDirectory);

            // Configure log4net
            var logConfig = new FileInfo(settings.Log4NetConfigPath);
            XmlConfigurator.Configure(logConfig);

            Log.Info($"{nameof(AppSettings)} have been successfully loaded.");
            AppSettings = settings; // Update static reference

            return settings;
        }
    }
}