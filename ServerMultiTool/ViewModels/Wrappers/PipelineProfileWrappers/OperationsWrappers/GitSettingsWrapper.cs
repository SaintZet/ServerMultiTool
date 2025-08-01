using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class GitPullOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    public string Name => "Git Pull";
    public string Description => "default description";

    readonly GitPullOperation _operation;

    public GitPullOperationWrapper(GitPullOperation operation)
    {
        _operation = operation;

        Enabled = operation.Enabled;
    }

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _operation.ExecuteAsync(cancellationToken);
    }

    public IPipelineOperation ToOriginal()
    {
        _operation.EnableOperation(Enabled);

        return _operation;
    }

    public void UpdateSolutionDirectory(DirectoryModel directory)
    {
        _operation.UpdateSolutionDirectory(directory);
    }

    public void UpdateHttpDirectory(DirectoryModel directory)
    {
        _operation.UpdateHttpDirectory(directory);
    }
}