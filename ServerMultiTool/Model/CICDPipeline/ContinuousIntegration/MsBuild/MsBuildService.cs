using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;
using ServerMultiTool.Model.Settings;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;

public static class MsBuildService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(MsBuildService));

    public static async Task ExecuteAsync(PipelineProfile pipeline)
    {
        var buildTasks = pipeline.SettingsPerProject
            .Where(projectSettings => projectSettings.MsBuildSettings.Enable)
            .Select(ExecuteMsBuildAsync);

        await Task.WhenAll(buildTasks);
    }

    private static async Task ExecuteMsBuildAsync(ProjectSettings projectSettings)
    {
        var projectPath = Path.Combine(AppSettingsService.AppSettings.SolutionDirectory, projectSettings.ProjectPath);
        var projectName = projectSettings.ProjectName;
        var buildParameters = projectSettings.MsBuildSettings.Parameters;
        var msBuildArguments = GetMsBuildArguments(projectPath, buildParameters);

        await ExecutePreBuildEventsAsync(projectSettings);
        
        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(projectPath),
            FileName = "msbuild",
            Arguments = msBuildArguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        Log.Info($"{projectName}: Process {startInfo.FileName} {startInfo.Arguments} has started.");
        
        if (await RunBuildWithRetriesAsync(startInfo, projectName, 2))
            await ExecutePostBuildEventsAsync(projectSettings);
    }

    private static async Task ExecutePreBuildEventsAsync(ProjectSettings projectSettings)
    {
        if (projectSettings.MsBuildSettings.PreBuildEvents.IsNullOrEmpty()) 
            return;
        
        foreach (var processEvent in projectSettings.MsBuildSettings.PreBuildEvents)
            await ExecuteProcessEventAsync(processEvent, projectSettings.ProjectName, "Pre");
    }

    private static async Task ExecutePostBuildEventsAsync(ProjectSettings projectSettings)
    {
        if (projectSettings.MsBuildSettings.PostBuildEvents.IsNullOrEmpty()) 
            return;

        foreach (var processEvent in projectSettings.MsBuildSettings.PostBuildEvents)
            await ExecuteProcessEventAsync(processEvent, projectSettings.ProjectName, "Post");
    }

    private static async Task ExecuteProcessEventAsync(ProcessEvent processEvent, string projectName, string eventType)
    {
        var processEventInfo = new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(processEvent.Path),
            FileName = processEvent.Path,
            Arguments = processEvent.Arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var (exitCode, output) = await RunProcessAndGetOutputAsync(processEventInfo);

        if (exitCode is 0)
        {
            Log.Info($"{projectName}: {eventType} process {processEventInfo.FileName} {processEventInfo.Arguments} has completed successfully.");
            return;
        }

        var errorMessage = $"{projectName}: {eventType} process {processEventInfo.FileName} {processEventInfo.Arguments} has failed.";
            
        if (output.IsNullOrEmpty() is false)
            errorMessage += $"\n{output}";
            
        Log.Error(errorMessage);
    }

    private static async Task<bool> RunBuildWithRetriesAsync(ProcessStartInfo startInfo, string projectName, int maxRetries)
    {
        var errorMessage = $"The build of project {projectName} has failed.";
        
        for (var retryCount = 1; retryCount <= maxRetries; retryCount++)
        {
            var response = await RunProcessAndGetOutputAsync(startInfo);
            
            if (response.ExitCode is 0)
            {
                Log.Info($"The build of project {projectName} has completed successfully.");
                return true;
            }

            if (response.Output.IsNullOrEmpty() is false)
                errorMessage += $"\n{response.Output}";
            
            Log.Error($"{projectName} is being used by another process. Retry {retryCount} of {maxRetries}.");
        }
        
        Log.Error(errorMessage);
        return false;
    }

    private static async Task<(int ExitCode, string Output)> RunProcessAndGetOutputAsync(ProcessStartInfo startInfo)
    {
        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return (process.ExitCode, output);
    }

    private static string GetMsBuildArguments(string projectPath, IEnumerable<string> parameters)
    {
        var sb = new StringBuilder()
            .Append(' ').Append($"\"{projectPath}\"")
            .Append(' ').Append($@"/p:SolutionDir={AppSettingsService.AppSettings.SolutionDirectory}\");

        foreach (var parameter in parameters) 
            sb.Append(' ').Append($"{parameter}");
        
        return sb.ToString();
    }
}
