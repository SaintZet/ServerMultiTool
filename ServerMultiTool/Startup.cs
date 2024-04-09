using System;
using System.IO;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Models.Build.Contracts;
using ServerMultiTool.Models.Build.Services;
using ServerMultiTool.Models.Deployment.Contracts;
using ServerMultiTool.Models.Deployment.Services;
using ServerMultiTool.Models.GeneralPipeline.Contracts;
using ServerMultiTool.Models.GeneralPipeline.Services;
using ServerMultiTool.Models.Integrations.Git.Contracts;
using ServerMultiTool.Models.Integrations.Git.Services;
using ServerMultiTool.Models.Integrations.Sql.Contracts;
using ServerMultiTool.Models.Integrations.Sql.Services;
using ServerMultiTool.Models.Integrations.WebBrowser.Contracts;
using ServerMultiTool.Models.Integrations.WebBrowser.Services;
using ServerMultiTool.Models.Settings.Global.Contracts;
using ServerMultiTool.Models.Settings.Global.Data;
using ServerMultiTool.Models.Settings.Global.Services;
using ServerMultiTool.Models.Settings.Profiles.Contracts;
using ServerMultiTool.Models.Settings.Profiles.Services;
using ServerMultiTool.ViewModels;

namespace ServerMultiTool;

public static class Startup
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .ConfigureServices()
            .AddViewModels();

        return services.BuildServiceProvider();
    }

    private static IServiceCollection ConfigureServices(this IServiceCollection serviceCollection)
    {
        var settingDirectory = GetOrCreateGlobalSettingsDirectory();

        XmlConfigurator.Configure(new FileInfo(GlobalSettingsConstants.LogSettingsFileName));
        
        serviceCollection
            .AddSingleton<IGlobalSettingsService>(new GlobalSettingsService(settingDirectory))
            .AddSingleton<ISettingsProfilesService, SettingsProfilesService>()
        
            .AddSingleton<IGeneralPipelineService, GeneralPipelineService>()
            
            .AddSingleton<IBuildService, BuildService>()
            .AddSingleton<IDeployService, DeployService>()
            .AddSingleton<IInternetInformationServices, InternetInformationServices>()
            
            .AddSingleton<IGitIntegrationService, GitIntegrationService>()
            .AddSingleton<ISqlExecutionService, SqlExecutionService>()
            .AddSingleton<IWebBrowserService, WebBrowserService>()
            ;

        return serviceCollection;
    }
    
    private static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddTransient<MainViewModel>();

        return serviceCollection;
    }

    private static string GetOrCreateGlobalSettingsDirectory()
    {
        var directory = Path.Combine(Environment.CurrentDirectory, GlobalSettingsConstants.SettingsFolderName);
        if (Directory.Exists(directory) is false) 
            Directory.CreateDirectory(directory);

        return directory;
    }
}