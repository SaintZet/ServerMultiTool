using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Contracts;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Pages.Pipeline.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Wrappers;

public class PipelineOperationWrapper(PipelineOperation operation, string displayName) : BaseObservableWrapper, IPipelineOperation
{
    private PipelineOperationStatus _pipelineOperationStatus = PipelineOperationStatus.Wait;
    public PipelineOperationStatus PipelineOperationStatus
    {
        get => _pipelineOperationStatus;
        private set => SetProperty(ref _pipelineOperationStatus, value);
    }

    public string DisplayName { get; } = displayName;

    public void UpdateSolutionDirectory(DirectoryModel directory) =>
        operation.UpdateSolutionDirectory(directory);

    public void UpdateHttpDirectory(DirectoryModel directory) =>
        operation.UpdateHttpDirectory(directory);

    public void OperationStarted() =>
        PipelineOperationStatus = PipelineOperationStatus.InProgress;

    public void ClearStatus() =>
        PipelineOperationStatus = PipelineOperationStatus.Wait;

    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = await operation.ExecuteAsync(cancellationToken);

        PipelineOperationStatus = OperationResultToStatus(result);

        return result;
    }

    private static PipelineOperationStatus OperationResultToStatus(OperationResult result) =>
        result switch
        {
            OperationResult.Failure => PipelineOperationStatus.Failure,
            OperationResult.Success or OperationResult.PartialSuccess or OperationResult.Cancelled => PipelineOperationStatus.Success,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
}