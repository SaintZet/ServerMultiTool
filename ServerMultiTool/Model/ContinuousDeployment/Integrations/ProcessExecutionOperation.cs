using ServerMultiTool.Model.Common.ProcessExecutor;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations
{
    public class ProcessExecutionOperation : BasePipelineOperation
    {
        public string FileName { get; private set; } = string.Empty;
        public string Arguments { get; private set; } = string.Empty;
        public int RetryCount { get; private set; } = 2;

        private readonly ProcessExecutor _processExecutor;

        public ProcessExecutionOperation(string name, string fileName)
            : base(name)
        {
            UpdateFileName(fileName);

            _processExecutor = new ProcessExecutor(Logger);
        }

        public ProcessExecutionOperation(string name, string fileName, string arguments)
            : base(name)
        {
            UpdateFileName(fileName);
            UpdateArguments(arguments);

            _processExecutor = new ProcessExecutor(Logger);
        }

        private ProcessExecutionOperation UpdateFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            FileName = fileName;
            return this;
        }

        public ProcessExecutionOperation UpdateArguments(string? arguments)
        {
            if (arguments is null)
                throw new ArgumentException("Arguments cannot be null.", nameof(arguments));

            Arguments = arguments;
            return this;
        }

        public ProcessExecutionOperation UpdateRetryCount(int retryCount)
        {
            if (retryCount < 0)
                throw new ArgumentException("Retry count cannot be negative.", nameof(retryCount));

            RetryCount = retryCount;
            return this;
        }

        protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            var info = new ProcessStartInfo(FileName, Arguments);
            var response = await _processExecutor.StartProcessWithRetriesAsync(info, RetryCount, cancellationToken);

            return response.Success ? PipelineOperationResult.Success : PipelineOperationResult.Failure;
        }
    }
}
