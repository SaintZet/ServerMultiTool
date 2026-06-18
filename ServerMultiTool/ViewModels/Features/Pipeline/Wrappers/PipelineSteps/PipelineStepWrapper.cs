using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using ServerMultiTool.Model.Features.Pipeline.Step;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Enums;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;

public partial class PipelineStepWrapper : ObservableObject
{
    readonly PipelineStep _pipelineStep;

    [ObservableProperty] public string _name;
    [ObservableProperty] public string _description;
    [ObservableProperty] int _order;

    [ObservableProperty] PipelineStepStatus _pipelineStepStatus = PipelineStepStatus.Wait;

    public PipelineOperationsCollection Operations { get; }

    public PipelineStepWrapper(PipelineStep pipelineStep)
    {
        _pipelineStep = pipelineStep ?? throw new ArgumentNullException(nameof(pipelineStep), "Pipeline step cannot be null.");

        Name = pipelineStep.Name;
        Description = pipelineStep.Description;
        Order = pipelineStep.Order;
        Operations = new PipelineOperationsCollection(pipelineStep.Operations);
    }

    public PipelineStep ToOriginal()
    {
        _pipelineStep.UpdateOrder(Order);
        _pipelineStep.UpdateName(Name);
        _pipelineStep.UpdateDescription(Description);
        return _pipelineStep;
    }

    public void AddOperation(PipelineOperationWrapper operation)
    {
        _pipelineStep.AddOperation(operation.ToOriginal());
        Operations.Add(operation);
    }

    public void RemoveOperation(PipelineOperationWrapper operation)
    {
        _pipelineStep.RemoveOperation(operation.ToOriginal());
        Operations.Remove(operation);
    }

    public void UpdateSolutionDirectory(DirectoryModel directory) =>
        Operations.UpdateSolutionDirectory(directory);

    public void UpdateHttpDirectory(DirectoryModel directory) =>
        Operations.UpdateHttpDirectory(directory);

    public void Started() =>
        PipelineStepStatus = PipelineStepStatus.InProgress;

    public void ClearStatus() =>
        PipelineStepStatus = PipelineStepStatus.Wait;

    public void Cancel() =>
        PipelineStepStatus = PipelineStepStatus.Cancelled;

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {

        var result = await Operations.ExecuteAsync(cancellationToken);

        PipelineStepStatus = OperationResultToStatus(result);

        return result;
    }

    private static PipelineStepStatus OperationResultToStatus(PipelineOperationResult result) =>
        result switch
        {
            PipelineOperationResult.Failure => PipelineStepStatus.Failure,
            PipelineOperationResult.Success => PipelineStepStatus.Success,
            PipelineOperationResult.PartialSuccess => PipelineStepStatus.Warning,
            PipelineOperationResult.Cancelled => PipelineStepStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
}