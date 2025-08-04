using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousDeployment.Delivery
{
    public class BaseDeliveryOperation : BasePipelineOperation
    {
        public DirectoryModel? Project { get; private set; }

        public BaseDeliveryOperation(string name)
            : base(name)
        {

        }

        public BaseDeliveryOperation(string name, DirectoryModel project)
            : base(name)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project), "Project cannot be null.");
        }

        protected override Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            // This method should be overridden in derived classes to implement specific delivery logic.
            throw new NotImplementedException("ExecuteOperationsAsync must be implemented in derived classes.");
        }

        private protected static async Task CopyDirectoryAsync(string sourceDirectory, string targetDirectory, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Directory.CreateDirectory(targetDirectory);

                foreach (var filePath in Directory.GetFiles(sourceDirectory))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fileName = Path.GetFileName(filePath);
                    var destFile = Path.Combine(targetDirectory, fileName);
                    File.Copy(filePath, destFile, overwrite: true);
                }

                foreach (var directoryPath in Directory.GetDirectories(sourceDirectory))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var directoryName = Path.GetFileName(directoryPath);
                    var destDirectory = Path.Combine(targetDirectory, directoryName);
                    await CopyDirectoryAsync(directoryPath, destDirectory, cancellationToken);
                }
            }, cancellationToken);
        }
    }
}
