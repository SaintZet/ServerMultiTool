using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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

        var mainWindow = new MainView
        {
            DataContext = _serviceProvider.GetService<MainViewModel>()
        };
        mainWindow.Show();
    }
        
}