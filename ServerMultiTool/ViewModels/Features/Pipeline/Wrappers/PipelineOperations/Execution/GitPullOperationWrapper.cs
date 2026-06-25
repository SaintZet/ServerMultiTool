using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Git;

public partial class GitPullOperationWrapper(GitPullOperation operation) : PipelineOperationWrapper(operation)
{
    public override string DefaultName => "Git Pull";
}
