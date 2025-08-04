using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Domain.Pipeline.Interfaces;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Collections
{
    public class PipelineOperationsCollection : ObservableCollection<IPipelineOperationWrapper>
    {
        public PipelineOperationsCollection(IEnumerable<IPipelineOperation> operations)
            : base(operations.Select(PipelineOperationWrapperFactory.Create))
        {

        }

        internal async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            foreach (var operation in this)
            {
                // check if the operation failed or was cancelled
                await operation.ExecuteAsync(cancellationToken);
            }

            return PipelineOperationResult.Success;
        }

        internal void UpdateHttpDirectory(DirectoryModel directory)
        {
            foreach (var operation in this)
            {
                operation.UpdateHttpDirectory(directory);
            }
        }

        internal void UpdateSolutionDirectory(DirectoryModel directory)
        {
            foreach (var operation in this)
            {
                operation.UpdateSolutionDirectory(directory);
            }
        }
    }
}
