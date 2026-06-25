using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Features.JsonParser.Presentation;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Common;

namespace ServerMultiTool.Features.JsonParser;

public static class JsonParserModuleServiceCollectionExtensions
{
    public static IServiceCollection AddJsonParserModule(this IServiceCollection services)
    {
        services.AddSingleton<JsonParserViewModel>();
        services.AddSingleton<JsonParserView>();
        services.AddSingleton<IPageDescriptor>(new PageDescriptor<JsonParserView>(
            AppRoutes.JsonParser.Key,
            AppRoutes.JsonParser.Title,
            IconKeys.Json,
            order: 10,
            showInMenu: false));

        return services;
    }
}

