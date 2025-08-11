using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Infrastructure.Services.Pipeline;
using ServerMultiTool.Model.Infrastructure.Services.Settings;
using ServerMultiTool.Views.Windows;
using System;
using System.Windows;

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
            PipelineProfilesContext.Instance.LoadOrInitialize(Environment.CurrentDirectory);

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