using ServerMultiTool.Model.Domain.Pipeline;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousDeployment.Delivery
{
    public abstract class BaseDeliveryOperation(string name) : PipelineOperation(name)
    {
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
