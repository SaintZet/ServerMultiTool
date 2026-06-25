using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Model.Infrastructure.DefaultValues;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Model.Infrastructure.Services;
using ServerMultiTool.ViewModels.Common;

namespace ServerMultiTool.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services)
    {
        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, AppConstants.Folders.AppSettings, "AppSettings.json");
        services.AddSingleton<IAppSettingsService>(_ => new FileAppSettingsService(appSettingsPath));

        var pipelineProfilesPath = Path.Combine(AppContext.BaseDirectory, AppConstants.Folders.AppSettings, AppConstants.Folders.Profiles);
        services.AddSingleton<IPipelineProfilesService>(_ => new FilePipelineProfilesService(pipelineProfilesPath));

        services.AddSingleton<IAutoUpdateService>(sp =>
        {
            var settingsService = sp.GetRequiredService<IAppSettingsService>();
            var appSettings = settingsService.Get();
            var feedUrl = string.IsNullOrWhiteSpace(appSettings.UpdateFeedUrl)
                ? DefaultUpdateSettings.AppcastUrl
                : appSettings.UpdateFeedUrl;
            var publicKey = string.IsNullOrWhiteSpace(appSettings.UpdatePublicKey)
                ? DefaultUpdateSettings.Ed25519PublicKey
                : appSettings.UpdatePublicKey;

            return new SparkleAutoUpdateService(
                feedUrl,
                publicKey,
                appSettings.CheckForUpdatesOnStartup);
        });

        return services;
    }
}

