using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Infrastructure.Services;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.Views.Windows;
using System;
using System.IO;
using System.Windows;

namespace ServerMultiTool;

public partial class App
{
    private readonly Logger _logger;

    public static FileAppSettingsService FileAppSettingsService { get; private set; }
    public static FilePipelineProfilesService FilePipelineProfilesService { get; private set; }

    public App()
    {
        _logger = new Logger(GetType());
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            var path = Path.Combine(Environment.CurrentDirectory, AppConstants.Folders.AppSettings, "AppSettings.json");
            FileAppSettingsService = new FileAppSettingsService(path);

            var path2 = Path.Combine(Environment.CurrentDirectory, AppConstants.Folders.AppSettings, AppConstants.Folders.Profiles);
            FilePipelineProfilesService = new FilePipelineProfilesService(path2);

            var mainWindow = new MainWindowView();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}