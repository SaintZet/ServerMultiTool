using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Data
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
