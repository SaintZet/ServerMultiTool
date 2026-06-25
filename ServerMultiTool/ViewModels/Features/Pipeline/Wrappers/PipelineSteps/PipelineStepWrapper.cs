using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using ServerMultiTool.Model.Features.Pipeline.Step;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Enums;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;

public partial class PipelineStepWrapper : ObservableObject
{
    readonly PipelineStep _pipelineStep;

    [ObservableProperty] public string _name;
    [ObservableProperty] public string _description;
    [ObservableProperty] int _order;
    [ObservableProperty] bool _enabled = true;

    [ObservableProperty] PipelineStepStatus _pipelineStepStatus = PipelineStepStatus.Wait;

    public PipelineOperationsCollection Operations { get; }

    public PipelineStepWrapper(PipelineStep pipelineStep)
    {
        _pipelineStep = pipelineStep ?? throw new ArgumentNullException(nameof(pipelineStep), "Pipeline step cannot be null.");

        Name = pipelineStep.Name;
        Description = pipelineStep.Description;
        Order = pipelineStep.Order;
        Enabled = pipelineStep.Enabled;
        Operations = new PipelineOperationsCollection(pipelineStep.Operations);

        foreach (var operation in Operations)
            operation.PropertyChanged += Operation_PropertyChanged;
    }

    public PipelineStep ToOriginal()
    {
        _pipelineStep.UpdateOrder(Order);
        _pipelineStep.UpdateName(Name);
        _pipelineStep.UpdateDescription(Description);
        _pipelineStep.EnableStep(Enabled);
        _pipelineStep.Operations = [.. Operations.Select(operation => operation.ToOriginal())];
        return _pipelineStep;
    }

    public void AddOperation(PipelineOperationWrapper operation)
    {
        operation.PropertyChanged += Operation_PropertyChanged;
        _pipelineStep.AddOperation(operation.ToOriginal());
        Operations.Add(operation);
    }

    public void RemoveOperation(PipelineOperationWrapper operation)
    {
        operation.PropertyChanged -= Operation_PropertyChanged;
        _pipelineStep.RemoveOperation(operation.ToOriginal());
        Operations.Remove(operation);
    }

    public event EventHandler? OperationFieldChanged;

    private void Operation_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OperationFieldChanged?.Invoke(this, EventArgs.Empty);
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
