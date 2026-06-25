using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Common;

namespace ServerMultiTool.Features.InternalTools;

public static class InternalToolsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddInternalToolsModule(this IServiceCollection services)
    {
        services.AddSingleton<InternalToolsViewModel>();
        services.AddSingleton<InternalToolsView>();
        services.AddSingleton<IPageDescriptor>(new PageDescriptor<InternalToolsView>(
            AppRoutes.InternalTools.Key,
            AppRoutes.InternalTools.Title,
            IconKeys.Analytics,
            order: 20,
            showInMenu: false));

        return services;
    }
}

