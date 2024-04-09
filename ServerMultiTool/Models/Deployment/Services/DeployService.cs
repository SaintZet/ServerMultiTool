using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Models.Build.Data;
using ServerMultiTool.Models.Deployment.Contracts;
using ServerMultiTool.Models.Deployment.Data;
using ServerMultiTool.Models.Settings.Global.Contracts;

namespace ServerMultiTool.Models.Deployment.Services;

public class DeployService : IDeployService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(DeployService));
    
    private readonly IGlobalSettingsService _globalSettingsService;

    public DeployService(IGlobalSettingsService globalSettingsService) => 
        _globalSettingsService = globalSettingsService;
    
    public async Task ExecuteDeployAsync(BuildSettings buildSettings, DeploySettings deploySettings)
    {
        if (!buildSettings.Enable || !deploySettings.Enable)
            return;
        
        var deployTasks = deploySettings.DeploySettingsPerProject
            .Where(project => project.ShouldDeploy)
            .Select(projectSettings => DeployProjectAsync(projectSettings, buildSettings))
            .ToList();

        await Task.WhenAll(deployTasks);
    }
    
    private async Task DeployProjectAsync(ProjectDeploySettings projectSettings, BuildSettings buildSettings)
    {
        var projectBuildSettings = buildSettings.BuildSettingsPerProject
            .Single(y => y.ProjectName == projectSettings.ProjectName); //TODO: Can be null

        var sourceDir = Path.Combine(Path.GetDirectoryName(projectBuildSettings.ProjectPath)!, "bin");
        var httpProjectDirectories = GetHttpProjectDirectories(projectSettings.ProjectName);

        var copyTasks = httpProjectDirectories.Select(targetDir => 
            Task.Run(() =>
            {
                var targetBinDir = Path.Combine(targetDir, "bin");
                CopyDirectory(sourceDir, targetBinDir);
                Log.Info($"Copy from {sourceDir} to {targetBinDir}");
            })).ToArray();

        await Task.WhenAll(copyTasks);
    }

    
    private IEnumerable<string> GetHttpProjectDirectories(string projectName)
    {
        var regex = new Regex(@$"{Regex.Escape(projectName)}\d*$");
        
        var allProjectsHttpDirectories = Directory.GetDirectories(_globalSettingsService.GlobalSettings.HttpDirectory);
        var projectHttpDirectories = allProjectsHttpDirectories.Where(path => regex.IsMatch(Path.GetFileName(path)));
        
        return projectHttpDirectories;
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
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
            CopyDirectory(directoryPath, destDirectory);
        }
    }
}
