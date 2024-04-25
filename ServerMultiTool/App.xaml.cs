using System;
using System.Windows;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.Views;

namespace ServerMultiTool;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppSettingsService.LoadOrInitialize(Environment.CurrentDirectory);
        PipelineProfilesService.LoadOrInitialize(Environment.CurrentDirectory);
        
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}