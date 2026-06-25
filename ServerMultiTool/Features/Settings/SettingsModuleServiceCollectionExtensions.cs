using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Features.Settings.Presentation;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Common;

namespace ServerMultiTool.Features.Settings;

public static class SettingsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<SettingsView>();
        services.AddSingleton<IPageDescriptor>(new PageDescriptor<SettingsView>(
            AppRoutes.Settings.Key,
            AppRoutes.Settings.Title,
            IconKeys.Settings,
            order: 100,
            menu: new MenuPresentationMetadata(Group: "System")));

        return services;
    }
}

