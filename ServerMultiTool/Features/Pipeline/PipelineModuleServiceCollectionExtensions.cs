using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Features.Pipeline.Presentation;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Common;

namespace ServerMultiTool.Features.Pipeline;

public static class PipelineModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPipelineModule(this IServiceCollection services)
    {
        services.AddSingleton<PipelineViewModel>();
        services.AddSingleton<PipelineView>();
        services.AddSingleton<IPageDescriptor>(new PageDescriptor<PipelineView>(
            AppRoutes.Pipeline.Key,
            AppRoutes.Pipeline.Title,
            IconKeys.Airplane,
            order: 0,
            menu: new MenuPresentationMetadata(Group: "Core")));

        return services;
    }
}

