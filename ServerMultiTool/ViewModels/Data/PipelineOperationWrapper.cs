using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.ViewModels.Data;

public class PipelineOperationWrapper : ObservableObject, IPipelineOperation
{
    private readonly PipelineOperation _operation;

    private PipelineOperationStatus _pipelineOperationStatus;
    public PipelineOperationStatus PipelineOperationStatus
    {
        get => _pipelineOperationStatus;
        private set => SetProperty(ref _pipelineOperationStatus, value);
    }

    public string DisplayName { get; }

    public PipelineOperationWrapper(PipelineOperation operation, string displayName)
    {
        _pipelineOperationStatus = PipelineOperationStatus.Wait;
        _operation = operation;

        DisplayName = displayName;
    }

    public void UpdateSolutionDirectory(DirectoryModel directory) =>
        _operation.UpdateSolutionDirectory(directory);

    public void UpdateHttpDirectory(DirectoryModel directory) =>
        _operation.UpdateHttpDirectory(directory);

    public async Task<OperationResult> ExecuteAsync()
    {
        var result = await _operation.ExecuteAsync();

        PipelineOperationStatus = OperationResultToStatus(result);

        return result;
    }

    private static PipelineOperationStatus OperationResultToStatus(OperationResult result)
    {
        switch (result)
        {
            case OperationResult.Failure:
                return PipelineOperationStatus.Failure;

            case OperationResult.Success:
            case OperationResult.PartialSuccess:
            case OperationResult.Cancelled:
                return PipelineOperationStatus.Success;

            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    public void ClearStatus() =>
        PipelineOperationStatus = PipelineOperationStatus.Wait;
}