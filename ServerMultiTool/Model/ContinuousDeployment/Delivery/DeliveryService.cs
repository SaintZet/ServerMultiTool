using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Delivery;

public class DeliveryService(IEnumerable<ProjectSettings> settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        try
        {
            var deliveryBinTasks = settings
                .Where(project => project.DeliverySettings.EnableDeliveryBin)
                .Select(project => DeliveryProjectBinAsync(project, cancellationToken));

            var deliverySpecificFilesTasks = settings
                .Where(project => project.DeliverySettings.EnableCustomDelivery &&
                                  project.DeliverySettings.CustomDeliveryDirectories.IsNullOrEmpty() is not true)
                .Select(project => DeliveryProjectSpecificFilesAsync(project, cancellationToken));

            var allDeliveryTasks = deliveryBinTasks.Concat(deliverySpecificFilesTasks);

            await Task.WhenAll(allDeliveryTasks);

            return OperationResult.Success;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled;
        }
    }

    private async Task DeliveryProjectSpecificFilesAsync(ProjectSettings projectSettings, CancellationToken cancellationToken)
    {
        await Task.WhenAll(projectSettings.DeliverySettings.CustomDeliveryDirectories!
            .Select(async directory =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (directory.Source.IsNullOrEmpty() || Path.GetDirectoryName(directory.Source) is null)
                    Logger.LogWarnWithPublish($"{projectSettings.Project.Name}: Wrong source directory for copy {directory.Source}");

                if (File.Exists(directory.Source))
                {
                    directory.Source = Path.GetDirectoryName(directory.Source)!;
                    Logger.LogWarnWithPublish($"{projectSettings.Project.Name}: Copying files directly is not supported. Getting directory: {directory.Source}");
                }

                if (Directory.Exists(directory.Source))
                {
                    await CopyDirectoryAsync(directory.Source, directory.Destination, cancellationToken);
                    Logger.LogInfoWithPublish($"{projectSettings.Project.Name}: Copy from {directory.Source} to {directory.Destination}");
                    return;
                }

                Logger.LogWarnWithPublish($"{projectSettings.Project.Name}: Cannot delivery {directory.Source} - directory not exist!");
            }));
    }

    private async Task DeliveryProjectBinAsync(ProjectSettings projectSettings, CancellationToken cancellationToken)
    {
        var projectDirectory = Path.GetDirectoryName(projectSettings.Project.Path)!;
        var fullProjectDirectory = Path.Combine(SolutionDirectory, projectDirectory);
        var sourceDirectory = Path.Combine(fullProjectDirectory, "bin");

        var copyTasks = GetHttpProjectDirectories(projectSettings.Project.Name)
            .Select(async directory =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var httpDirectory = Path.Combine(HttpDirectory, directory);
                var targetDirectory = Path.Combine(httpDirectory, "bin");

                await CopyDirectoryAsync(sourceDirectory, targetDirectory, cancellationToken);

                Logger.LogInfoWithPublish($"{projectSettings.Project.Name}: Copy from {sourceDirectory} to {targetDirectory}");
            });

        await Task.WhenAll(copyTasks);
    }

    private IEnumerable<string> GetHttpProjectDirectories(string folderName)
    {
        var regex = new Regex(@$"{Regex.Escape(folderName)}\d*$");

        var allProjectsHttpDirectories = Directory.GetDirectories(HttpDirectory);
        var projectHttpDirectories = allProjectsHttpDirectories.Where(path => regex.IsMatch(Path.GetFileName(path)));

        return projectHttpDirectories;
    }

    private static async Task CopyDirectoryAsync(string sourceDirectory, string targetDirectory, CancellationToken cancellationToken)
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