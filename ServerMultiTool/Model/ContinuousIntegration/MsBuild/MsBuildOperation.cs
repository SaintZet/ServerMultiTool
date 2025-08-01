using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.ProcessExecutor;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousIntegration.MsBuild;

public class MsBuildOperation : BasePipelineOperation
{
    public DirectoryModel Project { get; private set; }

    public List<string>? Parameters { get; set; }
    public List<ProcessEvent>? PreBuildEvents { get; set; }
    public List<ProcessEvent>? PostBuildEvents { get; set; }

    private readonly ProcessExecutor _processExecutor;

    public MsBuildOperation(string name, DirectoryModel project)
        : base(name)
    {
        Project = project;

        _processExecutor = new ProcessExecutor(Logger);
    }

    public MsBuildOperation AddParameter(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(parameter));

        Parameters ??= [];
        Parameters.Add(parameter);

        return this;
    }

    public MsBuildOperation AddPreBuildEvent(ProcessEvent processEvent)
    {
        if (processEvent is null)
            throw new ArgumentNullException(nameof(processEvent), "Process event cannot be null.");

        PreBuildEvents ??= [];
        PreBuildEvents.Add(processEvent);

        return this;
    }

    public MsBuildOperation AddPostBuildEvent(ProcessEvent processEvent)
    {
        if (processEvent is null)
            throw new ArgumentNullException(nameof(processEvent), "Process event cannot be null.");

        PostBuildEvents ??= [];
        PostBuildEvents.Add(processEvent);

        return this;
    }

    protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        const string fileName = "msbuild";

        var projectPath = Path.Combine(SolutionDirectory, Project.Path);
        var buildParameters = Parameters;

        var arguments = GetMsBuildArguments(projectPath, buildParameters);
        var workingDirectory = Path.GetDirectoryName(projectPath);

        await ExecutePreBuildEventsAsync(cancellationToken);

        var startInfo = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = workingDirectory };
        // todo: add retryCount parameter to settings
        var response = await _processExecutor.StartProcessWithRetriesAsync(startInfo, 1, cancellationToken);

        if (response.Success)
            await ExecutePostBuildEventsAsync(cancellationToken);

        return PipelineOperationResult.Success;
    }

    private async Task ExecutePreBuildEventsAsync(CancellationToken cancellationToken)
    {
        if (PreBuildEvents.IsNullOrEmpty())
            return;

        Logger.LogInfoWithPublish($"Start execute pre processes for {Project.Name}.");

        foreach (var processEvent in PreBuildEvents!)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteProcessEventAsync(processEvent, cancellationToken);
        }
    }

    private async Task ExecutePostBuildEventsAsync(CancellationToken cancellationToken)
    {
        if (PostBuildEvents.IsNullOrEmpty())
            return;

        Logger.LogInfoWithPublish($"Start execute post processes for {Project.Name}.");

        foreach (var processEvent in PostBuildEvents!)
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
        await _processExecutor.StartProcessOnceAsync(startInfo, cancellationToken);
    }

    private string GetMsBuildArguments(string projectPath, IEnumerable<string>? parameters)
    {
        if (parameters is null)
            return string.Empty;

        var dir = SolutionDirectory;
        if (dir.EndsWith("\\"))
        {
            dir += "\\";
        }
        else
        {
            dir += "\\\\";
        }

        var sb = new StringBuilder()
            .Append(' ').Append($"\"{projectPath}\"")
            .Append(' ').Append($"/p:SolutionDir=\"{dir}\"");

        foreach (var parameter in parameters)
            sb.Append(' ').Append($"{parameter}");

        return sb.ToString();
    }
}