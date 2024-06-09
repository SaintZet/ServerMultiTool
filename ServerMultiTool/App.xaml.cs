using System;
using System.Windows;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.Views;

namespace ServerMultiTool;

public partial class App
{
    private readonly Logger _logger;

    public App()
    {
        _logger = new Logger(GetType());
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            
            AppSettingsService.LoadOrInitialize(Environment.CurrentDirectory);
            PipelineProfilesService.LoadOrInitialize(Environment.CurrentDirectory);
        
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        catch (Exception exception)
        {
            _logger.LogException(exception);
        }
    }
}