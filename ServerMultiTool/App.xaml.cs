using System;
using System.IO;
using System.Windows;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.DependencyInjection;
using ServerMultiTool.Features;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Views.Windows;

namespace ServerMultiTool
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var logRepository = LogManager.GetRepository(typeof(App).Assembly);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            // Force updater service creation so periodic checks can start at app launch.
            _serviceProvider.GetRequiredService<IAutoUpdateService>();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindowView>();
            mainWindow.Show();
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
