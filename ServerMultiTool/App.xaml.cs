using System;
using System.IO;
using System.Windows;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.DependencyInjection;
using ServerMultiTool.Features;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Views.Windows;

namespace ServerMultiTool
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider = null!;
        private readonly Logger _startupLogger = new Logger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var logRepository = LogManager.GetRepository(typeof(App).Assembly);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindowView>();
            mainWindow.Show();

            _startupLogger.LogInfoWithPublish("Welcome aboard! Pipeline engines warmed up - let's ship something nice today.");

            // Start updater loop after main window is visible.
            _serviceProvider.GetRequiredService<IAutoUpdateService>().Start();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddApplicationInfrastructure()
                .AddShell()
                .AddFeatureModules();
        }
    }
}
