using ServerMultiTool.Model.Common.ProcessExecutor;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations
{
    public class IisResetOperation : BasePipelineOperation
    {
        public int RetryCount { get; private set; }
        public string Arguments { get; private set; } = string.Empty;

        private readonly ProcessExecutor _processExecutor;

        public IisResetOperation(string name, string arguments, int retryCount = 2)
            : base(name)
        {
            UpdateArguments(arguments);
            UpdateRetryCount(retryCount);

            _processExecutor = new ProcessExecutor(Logger);
        }

        public void UpdateRetryCount(int retryCount)
        {
            if (retryCount < 0)
                throw new ArgumentException("Retry count cannot be negative.", nameof(retryCount));

            RetryCount = retryCount;
        }

        public void UpdateArguments(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
                throw new ArgumentException("Arguments cannot be null or empty.", nameof(arguments));

            Arguments = arguments;
        }

        protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            var info = new ProcessStartInfo("iisreset.exe", Arguments);
            var response = await _processExecutor.StartProcessWithRetriesAsync(info, RetryCount, cancellationToken);

            return response.Success ? PipelineOperationResult.Success : PipelineOperationResult.Failure;
        }
    }
}
