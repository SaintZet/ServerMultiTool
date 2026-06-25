using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Features.InternalTools;
using ServerMultiTool.Features.JsonParser;
using ServerMultiTool.Features.Pipeline;
using ServerMultiTool.Features.Settings;

namespace ServerMultiTool.Features;

public static class FeatureModuleServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureModules(this IServiceCollection services)
    {
        return services
            .AddPipelineModule()
            .AddJsonParserModule()
            .AddInternalToolsModule()
            .AddSettingsModule();
    }
}

