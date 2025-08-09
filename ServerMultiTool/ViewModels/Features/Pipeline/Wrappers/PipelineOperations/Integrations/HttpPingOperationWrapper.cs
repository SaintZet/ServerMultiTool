using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class HttpPingOperationWrapper(HttpPingOperation operation) : PipelineOperationWrapper(operation)
{
    public override string Name { get; set; } = "HTTP Ping";
    public override string Description { get; set; } = "Pings a specified HTTP endpoint to check its availability.";
}