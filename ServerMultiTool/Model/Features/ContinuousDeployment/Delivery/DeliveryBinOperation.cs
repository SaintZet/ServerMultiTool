using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;

public class DeliveryBinOperation : BaseDeliveryOperation
{
    public DeliveryBinOperation(string name, DirectoryModel project)
        : base(name, project)
    {

    }

    protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        if (Project is null)
        {
            Logger.LogErrorWithPublish($"{Name}: Project is null.");
            return PipelineOperationResult.Failure;
        }

        var projectDirectory = Path.GetDirectoryName(Project.Path);

        if (string.IsNullOrEmpty(projectDirectory))
        {
            Logger.LogWarnWithPublish($"{Project.Name}: Project directory is null or empty.");
            return PipelineOperationResult.Failure;
        }

        var progectDirectory = Path.Combine(SolutionDirectory, projectDirectory);
        var sourceDirectory = Path.Combine(progectDirectory, "bin");

        var httpProjectDirectories = GetHttpProjectDirectories(Project.Name);

        if (httpProjectDirectories.Count() is 0)
        {
            Logger.LogWarnWithPublish($"{Project.Name}: not found http directories");
            return PipelineOperationResult.PartialSuccess;
        }

        var copyTasks = httpProjectDirectories
            .Select(async directory =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var httpDirectory = Path.Combine(HttpDirectory, directory);
                var destinationDirectory = Path.Combine(httpDirectory, "bin");

                await CopyDirectoryAsync(sourceDirectory, destinationDirectory, cancellationToken);

                Logger.LogInfoWithPublish($"Copy {Project.Name} from {sourceDirectory} to {destinationDirectory}");
            });

        await Task.WhenAll(copyTasks);

        return PipelineOperationResult.Success;
    }

    private IEnumerable<string> GetHttpProjectDirectories(string folderName)
    {
        var regex = new Regex(@$"{Regex.Escape(folderName)}\d*$");

        var allProjectsHttpDirectories = Directory.GetDirectories(HttpDirectory);
        var projectHttpDirectories = allProjectsHttpDirectories.Where(path => regex.IsMatch(Path.GetFileName(path)));

        return projectHttpDirectories;
    }
}