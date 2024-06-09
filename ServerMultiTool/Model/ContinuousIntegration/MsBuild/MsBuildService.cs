using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousIntegration.MsBuild;

public class MsBuildService : PipelineOperation
{
    private readonly ProjectSettings[] _settings;
    
    public MsBuildService(ProjectSettings[] settings) => 
        _settings = settings;

    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        var buildTasks = _settings
            .Where(settings => settings.MsBuildSettings.Enable)
            .Select(ExecuteMsBuildAsync);

        await Task.WhenAll(buildTasks);

        return OperationResult.Success;
    }

    private async Task ExecuteMsBuildAsync(ProjectSettings projectSettings)
    {
        const string fileName = "msbuild";
        
        var projectPath = Path.Combine(SolutionDirectory, projectSettings.Project.Path);
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
        
        Logger.LogInfoWithPublish($"Start execute pre processes for {projectSettings.Project.Name}.");
        
        foreach (var processEvent in projectSettings.MsBuildSettings.PreBuildEvents)
            await ExecuteProcessEventAsync(processEvent);
    }

    private async Task ExecutePostBuildEventsAsync(ProjectSettings projectSettings)
    {
        if (projectSettings.MsBuildSettings.PostBuildEvents.IsNullOrEmpty()) 
            return;
        
        Logger.LogInfoWithPublish($"Start execute post processes for {projectSettings.Project.Name}.");

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
