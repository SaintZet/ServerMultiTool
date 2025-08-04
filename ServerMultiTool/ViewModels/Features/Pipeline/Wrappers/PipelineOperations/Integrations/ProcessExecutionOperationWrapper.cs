using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Domain.Pipeline.Interfaces;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class ProcessExecutionOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    [ObservableProperty] int _retryCount;

    public string Name => "IIS Reset";
    public string Description => "Resets the IIS server to apply changes.";

    readonly ProcessExecutionOperation _operation;

    public ProcessExecutionOperationWrapper(ProcessExecutionOperation operation)
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