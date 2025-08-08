using ServerMultiTool.Model.Domain.Common.Logs;
using ServerMultiTool.Model.Domain.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousIntegration.Git
{
    public class GitPullOperation : PipelineOperation
    {
        private readonly Logger _logger;
        private readonly GitService _gitService = new();

        public override OperationType OperationType => OperationType.GitPull;

        public GitPullOperation(string name)
            : base(name)
        {
            _logger = new Logger(name);
        }

        protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            var output = await _gitService.GitPullAsync(SolutionDirectory, cancellationToken);

            if (output is null)
            {
                _logger.LogErrorWithPublish("Git pull operation failed or was cancelled.");
                return PipelineOperationResult.Failure;
            }

            if (output.Success)
                return PipelineOperationResult.Success;

            if (output.Output is not null)
                _logger.LogError(output.Output);

            _logger.LogErrorWithPublish("Something is going wrong. Check log details for more information.");
            return PipelineOperationResult.Failure;
        }
    }
}
