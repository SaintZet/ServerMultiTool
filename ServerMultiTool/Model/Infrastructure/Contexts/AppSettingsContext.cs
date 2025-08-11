using log4net;
using log4net.Config;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Model.Infrastructure.Repositories;
using System;
using System.IO;

namespace ServerMultiTool.Model.Infrastructure.Services.Settings;

public class AppSettingsContext
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(AppSettingsContext));
    private static readonly object _lockObject = new();
    private static AppSettingsContext? _instance;

    private IAppSettingsRepository? _repository;
    private string _appSettingsDirectory = null!;

    private AppSettingsContext() { }

    public static AppSettingsContext Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    _instance ??= new AppSettingsContext();
                }
            }
            return _instance;
        }
    }

    public AppSettings AppSettings { get; private set; } = default!;

    private IAppSettingsRepository Repository
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

    public AppSettingsContext LoadOrInitialize() =>
        LoadOrInitialize(AppDomain.CurrentDomain.BaseDirectory);

    public AppSettingsContext LoadOrInitialize(string appSettingsDirectory)
    {
        _appSettingsDirectory = appSettingsDirectory;
        var settings = Repository.LoadOrInitialize(appSettingsDirectory);

        // Configure log4net
        var logConfig = new FileInfo(settings.Log4NetConfigPath);
        XmlConfigurator.Configure(logConfig);

        Log.Info($"{nameof(AppSettings)} have been successfully loaded.");
        AppSettings = settings;

        return Instance;
    }

    public AppSettingsContext Update(AppSettings appSettings)
    {
        AppSettings = appSettings;
        Repository.Update(AppSettings);
        Log.Info($"{nameof(AppSettings)} have been successfully saved.");
        return Instance;
    }

    public AppSettingsContext SaveChanges()
    {
        Repository.Update(AppSettings);
        Log.Info($"{nameof(AppSettings)} have been successfully saved.");
        return Instance;
    }

    public AppSettingsContext UpdateField<T>(string fieldName, T value)
    {
        Repository.UpdateField(fieldName, value);
        AppSettings = Repository.Get(); // Refresh our copy
        Log.Info($"{nameof(AppSettings)} field '{fieldName}' has been successfully updated.");
        return Instance;
    }
}