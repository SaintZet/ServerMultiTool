using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousIntegration.MsBuild;

public class MsBuildService(IEnumerable<ProjectSettings> settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var buildTasks = settings
                .Where(s => s.MsBuildSettings.Enable)
                .Select(s => ExecuteMsBuildAsync(s, cancellationToken));

            await Task.WhenAll(buildTasks);

            return OperationResult.Success;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled;
        }
    }

    private async Task ExecuteMsBuildAsync(ProjectSettings projectSettings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string fileName = "msbuild";

        var projectPath = Path.Combine(SolutionDirectory, projectSettings.Project.Path);
        var buildParameters = projectSettings.MsBuildSettings.Parameters;

        var arguments = GetMsBuildArguments(projectPath, buildParameters);
        var workingDirectory = Path.GetDirectoryName(projectPath);

        await ExecutePreBuildEventsAsync(projectSettings, cancellationToken);

        var startInfo = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = workingDirectory };
        // todo: add retryCount parameter to settings
        var response = await ProcessExecutor.StartProcessWithRetriesAsync(startInfo, 10, cancellationToken);

        if (response.Success)
            await ExecutePostBuildEventsAsync(projectSettings, cancellationToken);
    }

    private async Task ExecutePreBuildEventsAsync(ProjectSettings projectSettings, CancellationToken cancellationToken)
    {
        if (projectSettings.MsBuildSettings.PreBuildEvents.IsNullOrEmpty())
            return;

        Logger.LogInfoWithPublish($"Start execute pre processes for {projectSettings.Project.Name}.");

        foreach (var processEvent in projectSettings.MsBuildSettings.PreBuildEvents!)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteProcessEventAsync(processEvent, cancellationToken);
        }
    }

    private async Task ExecutePostBuildEventsAsync(ProjectSettings projectSettings, CancellationToken cancellationToken)
    {
        if (projectSettings.MsBuildSettings.PostBuildEvents.IsNullOrEmpty())
            return;

        Logger.LogInfoWithPublish($"Start execute post processes for {projectSettings.Project.Name}.");

        foreach (var processEvent in projectSettings.MsBuildSettings.PostBuildEvents!)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteProcessEventAsync(processEvent, cancellationToken);
        }
    }

    private async Task ExecuteProcessEventAsync(ProcessEvent processEvent, CancellationToken cancellationToken)
    {
        if (processEvent.Arguments is null)
            return;

        var startInfo = new ProcessStartInfo(processEvent.Path, processEvent.Arguments);
        await ProcessExecutor.StartProcessOnceAsync(startInfo, cancellationToken);
    }

    private string GetMsBuildArguments(string projectPath, IEnumerable<string>? parameters)
    {
        if (parameters is null)
            return string.Empty;

        var sb = new StringBuilder()
            .Append(' ').Append($"\"{projectPath}\"")
            .Append(' ').Append($"/p:SolutionDir=\"{SolutionDirectory}\"");

        foreach (var parameter in parameters)
            sb.Append(' ').Append($"{parameter}");

        return sb.ToString();
    }
}