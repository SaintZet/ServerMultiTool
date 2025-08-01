using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers
{
    public interface IPipelineOperationWrapper
    {
        public bool Enabled { get; set; }
        public string Name { get; }
        public string Description { get; }

        public void UpdateSolutionDirectory(DirectoryModel directory);

        public void UpdateHttpDirectory(DirectoryModel directory);

        Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken);
        public IPipelineOperation ToOriginal();
    }
}