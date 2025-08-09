using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base
{
    public abstract partial class PipelineOperationWrapper : ObservableObject
    {
        [ObservableProperty] bool _enabled;

        public PipelineOperation Operation { get; }

        protected PipelineOperationWrapper(PipelineOperation operation)
        {
            Operation = operation;
            Enabled = operation.Enabled;
        }

        public abstract string Name { get; set; }
        public abstract string Description { get; set; }

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

        public virtual PipelineOperation ToOriginal()
        {
            Operation.EnableOperation(Enabled);
            return Operation;
        }
    }
}