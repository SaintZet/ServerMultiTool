using ServerMultiTool.Model.Features.ContinuousIntegration.Git;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Git;

public partial class GitPullOperationWrapper(GitPullOperation operation) : PipelineOperationWrapper(operation)
{
    public override string Name { get; set; } = "Git Pull";
    public override string Description { get; set; } = "default description";
}