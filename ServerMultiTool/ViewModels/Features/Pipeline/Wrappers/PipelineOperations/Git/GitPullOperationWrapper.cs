using ServerMultiTool.Model.Features.ContinuousIntegration.Git;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Git;

public partial class GitPullOperationWrapper(GitPullOperation operation) : PipelineOperationWrapper(operation)
{
    public override string Name => "Git Pull";
    public override string Description => "default description";
}