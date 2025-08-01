using ServerMultiTool.Model.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Pipeline.Contracts
{
    public interface IPipelineOperation
    {
        public Guid Guid { get; }
        public bool Enabled { get; }
        public string SolutionDirectory { get; }
        public string HttpDirectory { get; }

        public Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken);

        public void UpdateSolutionDirectory(DirectoryModel directory);

        public void UpdateHttpDirectory(DirectoryModel directory);

        public void EnableOperation(bool enable);
    }
}