using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base
{
    public abstract partial class PipelineOperationWrapper : ObservableObject
    {
        [ObservableProperty] bool _enabled;
        [ObservableProperty] string _name;
        [ObservableProperty] string _description;
        [ObservableProperty] bool _failStepOnFailure;

        public PipelineOperationBase Operation { get; }

        protected PipelineOperationWrapper(PipelineOperationBase operation)
        {
            Operation = operation;
            Enabled = operation.Enabled;
            Name = operation.Name ?? DefaultName;
            Description = operation.Description ?? string.Empty;
            FailStepOnFailure = operation.FailStepOnFailure;
        }

        partial void OnEnabledChanged(bool value)
        {
            Operation.EnableOperation(value);
        }

        partial void OnFailStepOnFailureChanged(bool value)
        {
            Operation.SetFailStepOnFailure(value);
        }

        partial void OnDescriptionChanged(string value)
        {
            Operation.UpdateDescription(value);
        }

        public abstract string DefaultName { get; }

        public virtual void UpdateSolutionDirectory(DirectoryModel directory)
        {
            Operation.UpdateSolutionDirectory(directory);
        }

        public virtual void UpdateHttpDirectory(DirectoryModel directory)
        {
            Operation.UpdateHttpDirectory(directory);
        }

        public virtual async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await Operation.ExecuteAsync(cancellationToken);
        }

        public virtual PipelineOperationBase ToOriginal()
        {
            Operation.EnableOperation(Enabled);
            Operation.SetFailStepOnFailure(FailStepOnFailure);
            return Operation;
        }
    }
}
