using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.ProcessExecutor;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;

namespace ServerMultiTool.Model.Features.Pipeline.Operations.Execution;

public class MsBuildOperation : PipelineOperationBase
{
    public override OperationType OperationType => OperationType.MsBuildOperation;

    [JsonInclude] public DirectoryModel Project { get; private set; } = new DirectoryModel();
    [JsonInclude] public int RetryCount { get; private set; } = 1;
    [JsonInclude] public List<string>? Parameters { get; private set; }

    private readonly ProcessExecutor _processExecutor;

    public MsBuildOperation(string name)
            : base(name)
    {
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

    public MsBuildOperation UpdateParameters(IEnumerable<string>? parameters)
    {
        Parameters = parameters?.ToList();
        return this;
    }

    public MsBuildOperation UpdateRetryCount(int retryCount)
    {
        if (retryCount < 0)
            throw new ArgumentException("Retry count cannot be negative.", nameof(retryCount));

        RetryCount = retryCount;
        return this;
    }

    public MsBuildOperation UpdateProject(DirectoryModel project)
    {
        if (project is null || string.IsNullOrWhiteSpace(project.Path))
            throw new ArgumentException("Project cannot be null or empty.", nameof(project));

        Project = project;
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
