using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base
{
    public abstract partial class PipelineOperationWrapper : ObservableObject
    {
        [ObservableProperty] bool _enabled;
        [ObservableProperty] string _name;

        public PipelineOperationBase Operation { get; }

        protected PipelineOperationWrapper(PipelineOperationBase operation)
        {
            Operation = operation;
            Enabled = operation.Enabled;
            Name = operation.Name ?? DefaultName;
        }

        partial void OnEnabledChanged(bool value)
        {
            Operation.EnableOperation(value);
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
            return Operation;
        }
    }
}