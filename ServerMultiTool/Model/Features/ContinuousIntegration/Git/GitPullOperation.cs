using ServerMultiTool.Model.Domain.Common.Logs;
using ServerMultiTool.Model.Domain.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousIntegration.Git
{
    public class GitPullOperation(string name) : PipelineOperation(name)
    {
        public override OperationType OperationType => OperationType.GitPullOperation;

        private readonly GitService _gitService = new();

        protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            var output = await _gitService.GitPullAsync(SolutionDirectory, cancellationToken);

            if (output is null)
            {
                Logger.LogErrorWithPublish("Git pull operation failed or was cancelled.");
                return PipelineOperationResult.Failure;
            }

            if (output.Success)
                return PipelineOperationResult.Success;

            if (output.Output is not null)
                Logger.LogError(output.Output);

            Logger.LogErrorWithPublish("Something is going wrong. Check log details for more information.");
            return PipelineOperationResult.Failure;
        }
    }
}
