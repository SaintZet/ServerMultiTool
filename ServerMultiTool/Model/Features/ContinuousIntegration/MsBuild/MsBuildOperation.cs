using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Common.ProcessExecutor;
using ServerMultiTool.Model.Domain.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousIntegration.MsBuild;

public class MsBuildOperation : PipelineOperation
{
    public DirectoryModel Project { get; private set; }

    public int RetryCount { get; private set; } = 1;
    public List<string>? Parameters { get; private set; }

    public override OperationType OperationType => OperationType.MsBuildOperation;

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

    public MsBuildOperation UpdateRetryCount(int retryCount)
    {
        if (retryCount < 0)
            throw new ArgumentException("Retry count cannot be negative.", nameof(retryCount));

        RetryCount = retryCount;
        return this;
    }

    protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        const string fileName = "msbuild";

        var projectPath = Path.Combine(SolutionDirectory, Project.Path);
        var buildParameters = Parameters;

        var arguments = GetMsBuildArguments(projectPath, buildParameters);
        var workingDirectory = Path.GetDirectoryName(projectPath);

        var startInfo = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = workingDirectory };
        var response = await _processExecutor.StartProcessWithRetriesAsync(startInfo, 1, cancellationToken);

        return PipelineOperationResult.Success;
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