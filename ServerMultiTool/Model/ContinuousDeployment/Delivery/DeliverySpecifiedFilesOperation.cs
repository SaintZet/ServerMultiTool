using ServerMultiTool.Model.Pipeline;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Delivery
{
    public class DeliverySpecifiedFilesOperation : BaseDeliveryOperation
    {
        public List<DeliveryDirectory> CustomDeliveryDirectories { get; set; } = [];

        public DeliverySpecifiedFilesOperation(string name)
            : base(name)
        {

        }

        public DeliverySpecifiedFilesOperation AddDeliveryDirectories(string source, string destination)
        {
            var deliveryDirectory = new DeliveryDirectory
            {
                Source = source,
                Destination = destination
            };

            CustomDeliveryDirectories.Add(deliveryDirectory);
            return this;
        }

        protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(CustomDeliveryDirectories
                .Select(async directory =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrEmpty(directory.Source) || Path.GetDirectoryName(directory.Source) is null)
                        Logger.LogWarnWithPublish($"Wrong source directory for copy {directory.Source}");

                    if (File.Exists(directory.Source))
                    {
                        directory.Source = Path.GetDirectoryName(directory.Source)!;
                        Logger.LogWarnWithPublish($"Copying files directly is not supported. Getting directory: {directory.Source}");
                    }

                    if (Directory.Exists(directory.Source))
                    {
                        await CopyDirectoryAsync(directory.Source, directory.Destination, cancellationToken);
                        Logger.LogInfoWithPublish($"Copy from {directory.Source} to {directory.Destination}");
                        return;
                    }

                    Logger.LogWarnWithPublish($"Cannot delivery {directory.Source} - directory not exist!");
                }));

            return PipelineOperationResult.Success;
        }
    }
}
