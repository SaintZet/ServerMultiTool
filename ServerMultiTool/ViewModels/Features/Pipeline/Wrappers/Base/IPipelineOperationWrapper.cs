using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base
{
    public interface IPipelineOperationWrapper
    {
        public bool Enabled { get; set; }
        public string Name { get; }
        public string Description { get; }

        public void UpdateSolutionDirectory(DirectoryModel directory);

        public void UpdateHttpDirectory(DirectoryModel directory);

        Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken);
        public PipelineOperation ToOriginal();
    }
}