using System;
using System.Threading.Tasks;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Contracts;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Pages.Pipeline.Enums;

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

    public async Task<OperationResult> ExecuteAsync()
    {
        var result = await operation.ExecuteAsync();

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

    public void ClearStatus() =>
        PipelineOperationStatus = PipelineOperationStatus.Wait;
}