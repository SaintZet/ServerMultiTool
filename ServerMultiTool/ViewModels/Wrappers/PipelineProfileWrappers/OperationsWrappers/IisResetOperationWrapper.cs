using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.Pipeline.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class IisResetOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    [ObservableProperty] int _retryCount;

    public string Name => "IIS Reset";
    public string Description => "Resets the IIS server to apply changes.";

    readonly IisResetOperation _operation;

    public IisResetOperationWrapper(IisResetOperation operation)
    {
        _operation = operation;

        Enabled = operation.Enabled;
        RetryCount = operation.RetryCount;
    }

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _operation.ExecuteAsync(cancellationToken);
    }

    public IPipelineOperation ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        _operation.UpdateRetryCount(RetryCount);

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