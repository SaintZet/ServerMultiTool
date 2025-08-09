using ServerMultiTool.Model.Features.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.MsBuild;

public partial class MsBuildOperationWrapper(MsBuildOperation operation) : PipelineOperationWrapper(operation)
{
    public override string Name { get; set; } = "HTTP Ping";
    public override string Description { get; set; } = "Pings a specified HTTP endpoint to check its availability.";
}
