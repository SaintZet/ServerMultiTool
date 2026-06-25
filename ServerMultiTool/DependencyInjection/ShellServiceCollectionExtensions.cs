using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Shared.Components.GeneralInfo;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Windows;
using ServerMultiTool.Views.Windows;

namespace ServerMultiTool.DependencyInjection;

public static class ShellServiceCollectionExtensions
{
    public static IServiceCollection AddShell(this IServiceCollection services)
    {
        services.AddSingleton(new ShellOptions
        {
            StartupRoute = AppRoutes.Pipeline,
            StartupPageKey = AppRoutes.Pipeline.Key
        });

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<GeneralInfoViewModel>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindowView>();

        return services;
    }
}

