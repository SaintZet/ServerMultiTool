using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;

public class MsBuildService : ExecutionService
{
    public string SolutionDirectory;

    public MsBuildService(string solutionDirectory) => 
        SolutionDirectory = solutionDirectory;


    public async Task<bool> ExecuteAsync(PipelineProfile pipeline)
    {
        var buildTasks = pipeline.SettingsPerProject
            .Where(projectSettings => projectSettings.MsBuildSettings.Enable)
            .Select(ExecuteMsBuildAsync);

        await Task.WhenAll(buildTasks);

        return true;
    }

    private async Task ExecuteMsBuildAsync(ProjectSettings projectSettings)
    {
        const string fileName = "msbuild";
        
        var projectPath = Path.Combine(SolutionDirectory, projectSettings.ProjectPath);
        var buildParameters = projectSettings.MsBuildSettings.Parameters;
        
        var arguments = GetMsBuildArguments(projectPath, buildParameters);
        var workingDirectory = Path.GetDirectoryName(projectPath);

        await ExecutePreBuildEventsAsync(projectSettings);
        
        var startInfo = new ProcessStartInfo(fileName, arguments) { WorkingDirectory =  workingDirectory};
        var response = await ProcessExecutor.StartProcessWithRetriesAsync(startInfo, 10);
        
        if (response.Success)
            await ExecutePostBuildEventsAsync(projectSettings);
    }

    private async Task ExecutePreBuildEventsAsync(ProjectSettings projectSettings)
    {
        if (projectSettings.MsBuildSettings.PreBuildEvents.IsNullOrEmpty()) 
            return;
        
        Logger.LogInfo("Start execute pre processes for {projectSettings.ProjectName}.");
        
        foreach (var processEvent in projectSettings.MsBuildSettings.PreBuildEvents)
            await ExecuteProcessEventAsync(processEvent);
    }

    private async Task ExecutePostBuildEventsAsync(ProjectSettings projectSettings)
    {
        if (projectSettings.MsBuildSettings.PostBuildEvents.IsNullOrEmpty()) 
            return;
        
        Logger.LogInfo("Start execute post processes for {projectSettings.ProjectName}.");

        foreach (var processEvent in projectSettings.MsBuildSettings.PostBuildEvents)
            await ExecuteProcessEventAsync(processEvent);
    }

    private async Task ExecuteProcessEventAsync(ProcessEvent processEvent)
    {
        var startInfo = new ProcessStartInfo(processEvent.Path, processEvent.Arguments);

        await ProcessExecutor.StartProcessOnceAsync(startInfo);
    }

    private string GetMsBuildArguments(string projectPath, IEnumerable<string> parameters)
    {
        var sb = new StringBuilder()
            .Append(' ').Append($"\"{projectPath}\"")
            .Append(' ').Append($@"/p:SolutionDir={SolutionDirectory}\");

        foreach (var parameter in parameters) 
            sb.Append(' ').Append($"{parameter}");
        
        return sb.ToString();
    }
}
