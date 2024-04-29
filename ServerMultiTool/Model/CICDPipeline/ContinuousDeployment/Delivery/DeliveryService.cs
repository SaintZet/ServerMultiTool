using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Delivery;

public class DeliveryService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(DeliveryService));

    private readonly string _solutionDirectory;
    private readonly string _httpDirectory;

    public DeliveryService(string solutionDirectory, string httpDirectory)
    {
        _solutionDirectory = solutionDirectory;
        _httpDirectory = httpDirectory;
    }

    public async Task ExecuteAsync(PipelineProfile pipeline)
    {
        var deliveryBinTasks = pipeline.SettingsPerProject
            .Where(projectSettings => projectSettings.DeliverySettings.DeliveryBin)
            .Select(DeliveryProjectBinAsync)
            .ToList();

        var deliverySpecificFilesTasks = pipeline.SettingsPerProject
            .Where(projectSettings => projectSettings.DeliverySettings.DeliveryDirectory.IsNullOrEmpty() is false)
            .Select(DeliveryProjectSpecificFilesAsync)
            .ToList();
        
        var allDeliveryTasks = deliveryBinTasks.Concat(deliverySpecificFilesTasks);
        
        await Task.WhenAll(allDeliveryTasks);
    }

    private static async Task DeliveryProjectSpecificFilesAsync(ProjectSettings projectSettings)
    {
        await Task.WhenAll(projectSettings.DeliverySettings.DeliveryDirectory
            .Select(async directory =>
            {
                if (directory.Source.IsNullOrEmpty() || Path.GetDirectoryName(directory.Source) is null)
                    Log.Warn($"{projectSettings.ProjectName}: Wrong source directory for copy {directory.Source}");

                if (File.Exists(directory.Source))
                {
                    directory.Source = Path.GetDirectoryName(directory.Source)!;
                    Log.Warn($"{projectSettings.ProjectName}: Copying files directly is not supported. Getting directory: {directory.Source}");
                }

                if (Directory.Exists(directory.Source))
                {
                    await CopyDirectoryAsync(directory.Source, directory.Destination);
                    Log.Info($"{projectSettings.ProjectName}: Copy from {directory.Source} to {directory.Destination}");
                    return;
                }
                
                Log.Warn($"{projectSettings.ProjectName}: Cannot delivery {directory.Source} - directory not exist!");
            }));
    }
    
    private async Task DeliveryProjectBinAsync(ProjectSettings projectSettings)
    {
        var projectDirectory = Path.GetDirectoryName(projectSettings.ProjectPath)!;
        var fullProjectDirectory = Path.Combine(_solutionDirectory, projectDirectory);
        var sourceDirectory = Path.Combine(fullProjectDirectory, "bin");

        var copyTasks = GetHttpProjectDirectories(projectSettings.ProjectName)
            .Select(async directory =>
            {
                var httpDirectory = Path.Combine(_httpDirectory, directory);
                var targetDirectory = Path.Combine(httpDirectory, "bin");
                
                await CopyDirectoryAsync(sourceDirectory, targetDirectory);
                
                Log.Info($"{projectSettings.ProjectName}: Copy from {sourceDirectory} to {targetDirectory}");
            });

        await Task.WhenAll(copyTasks);
    }
    
    private IEnumerable<string> GetHttpProjectDirectories(string folderName)
    {
        var regex = new Regex(@$"{Regex.Escape(folderName)}\d*$");
        
        var allProjectsHttpDirectories = Directory.GetDirectories(_httpDirectory);
        var projectHttpDirectories = allProjectsHttpDirectories.Where(path => regex.IsMatch(Path.GetFileName(path)));
        
        return projectHttpDirectories;
    }

    private static async Task CopyDirectoryAsync(string sourceDirectory, string targetDirectory)
    {
        await Task.Run(async () =>
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (var filePath in Directory.GetFiles(sourceDirectory))
            {
                var fileName = Path.GetFileName(filePath);
                var destFile = Path.Combine(targetDirectory, fileName);
                File.Copy(filePath, destFile, overwrite: true);
            }

            foreach (var directoryPath in Directory.GetDirectories(sourceDirectory))
            {
                var directoryName = Path.GetFileName(directoryPath);
                var destDirectory = Path.Combine(targetDirectory, directoryName);
                await CopyDirectoryAsync(directoryPath, destDirectory);
            }
        });
    }
}
