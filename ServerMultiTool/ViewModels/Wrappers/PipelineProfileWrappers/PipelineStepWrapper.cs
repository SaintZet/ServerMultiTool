using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using ServerMultiTool.ViewModels.Pages.Pipeline.Enums;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers
{
    public partial class PipelineStepWrapper : ObservableObject
    {
        readonly PipelineStep _pipelineStep;

        public string DisplayName => _pipelineStep.Name;

        public string Description => _pipelineStep.Description;

        [ObservableProperty] int _order;

        [ObservableProperty] PipelineStepStatus _pipelineStepStatus = PipelineStepStatus.Wait;

        public PipelineOperationsCollection Operations { get; }

        public PipelineStepWrapper(PipelineStep pipelineStep)
        {
            _pipelineStep = pipelineStep ?? throw new ArgumentNullException(nameof(pipelineStep), "Pipeline step cannot be null.");

            Operations = new PipelineOperationsCollection(pipelineStep.Operations);

            Order = pipelineStep.Order;
        }

        public PipelineStep ToOriginal()
        {
            _pipelineStep.UpdateOrder(Order);
            return _pipelineStep;
        }

        public void AddOperation(IPipelineOperationWrapper operation)
        {
            _pipelineStep.AddOperation(operation.ToOriginal());
            Operations.Add(operation);
        }

        public void RemoveOperation(IPipelineOperationWrapper operation)
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
}