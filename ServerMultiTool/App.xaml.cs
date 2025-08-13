using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Model.Infrastructure.Services;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Components.GeneralInfo;
using ServerMultiTool.ViewModels.Features.JsonParser;
using ServerMultiTool.ViewModels.Pages.Pipeline;
using ServerMultiTool.ViewModels.Pages.Settings;
using ServerMultiTool.ViewModels.Windows;
using ServerMultiTool.Views.Pages;
using ServerMultiTool.Views.Windows;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ServerMultiTool
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindowView>();
            mainWindow.Show();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Register services
            var appSettingsPath = Path.Combine(AppContext.BaseDirectory, AppConstants.Folders.AppSettings, "AppSettings.json");
            services.AddSingleton<IAppSettingsService>(sp =>
            {
                return new FileAppSettingsService(appSettingsPath);
            });

            var pipelineProfilesPath = Path.Combine(AppContext.BaseDirectory, AppConstants.Folders.AppSettings, AppConstants.Folders.Profiles);
            services.AddSingleton<IPipelineProfilesService>(sp =>
            {
                return new FilePipelineProfilesService(pipelineProfilesPath);
            });

            // Register view models
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<GeneralInfoViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<JsonParserViewModel>();
            services.AddSingleton(sp =>
            {
                var appSettingsService = sp.GetRequiredService<IAppSettingsService>();
                var profilesService = sp.GetRequiredService<IPipelineProfilesService>();
                var generalInfo = sp.GetRequiredService<GeneralInfoViewModel>();
                var settingsViewModel = sp.GetRequiredService<SettingsViewModel>();

                return new PipelineViewModel(appSettingsService, profilesService, generalInfo)
                {
                    // todo: change to services.AddSingleton<ISettingsNavigationService, SettingsNavigationService>();
                    NavigateToSettingsAction = (tabKey, param) =>
                    {
                        if (string.IsNullOrEmpty(tabKey))
                            return;

                        settingsViewModel.SelectedTabKey = tabKey;

                        if (tabKey is SettingsPageTabKeys.PipelineProfiles && string.IsNullOrEmpty(param) is false)
                            settingsViewModel.SelectedPipelineProfile = settingsViewModel.PipelineProfiles.First(x => x.Name == param);

                        // You'll need to handle navigation differently with DI
                        // Consider using a navigation service
                    }
                };
            });

            // View
            services.AddSingleton<MainWindowView>();
            services.AddSingleton<PipelineView>();
            services.AddSingleton<SettingsView>();
            services.AddSingleton<JsonParserView>();
        }
    }
}
