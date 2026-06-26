using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Collections
{
    public class PipelineOperationsCollection : ObservableCollection<PipelineOperationWrapper>
    {
        public PipelineOperationsCollection(IEnumerable<PipelineOperationBase> operations)
            : base(operations.Select(PipelineOperationWrapperFactory.Create))
        {

        }

        internal async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            var finalResult = PipelineOperationResult.Success;

            foreach (var operation in this)
            {
                if (!operation.Enabled)
                    continue;

                var result = await operation.ExecuteAsync(cancellationToken);

                if (result == PipelineOperationResult.Cancelled)
                    return result;

                if (result == PipelineOperationResult.Failure)
                {
                    // дальнейшие операции не запускаем в любом случае
                    // но степ фейлим только если настроено
                    return operation.FailStepOnFailure
                        ? PipelineOperationResult.Failure
                        : PipelineOperationResult.PartialSuccess;
                }

                if (result == PipelineOperationResult.PartialSuccess)
                    finalResult = PipelineOperationResult.PartialSuccess;
            }

            return finalResult;
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
