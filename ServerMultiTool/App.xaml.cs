using System;
using System.Windows;
using ServerMultiTool.ViewModels;
using ServerMultiTool.Views;

namespace ServerMultiTool;

public partial class App
{
    private readonly IServiceProvider _serviceProvider;
    
    public App() => 
        _serviceProvider = ServerMultiTool.Startup.ConfigureServices();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DeployViewModel.SetServiceProvider(_serviceProvider);
        
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
        
}