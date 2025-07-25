using ServerMultiTool.Model.Common.ProcessExecutor;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousIntegration.Git;

public class GitService : PipelineOperation
{
    private readonly GitSettings? _settings;

    public GitService() { }

    public GitService(GitSettings? settings) =>
        _settings = settings;

    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        try
        {
            ProcessOutput? output = null;

            if (_settings!.ShouldPull is false)
            {
                return OperationResult.Success; // git pull is not required and the only one operation right now
            }

            output = await GitPull(cancellationToken);

            if (output is null)
            {
                Logger.LogErrorWithPublish("Git pull operation failed or was cancelled.");
                return OperationResult.Failure;
            }

            if (output.Success is false)
            {
                if (output.Output is not null)
                    Logger.LogErrorWithPublish(output.Output);

                return OperationResult.Failure;
            }

            return OperationResult.Success;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled;
        }
    }

    public async Task<string?> GetCurrentBranchName(string solutionDirectory)
    {
        const string fileName = "git";
        const string arguments = "rev-parse --abbrev-ref HEAD";

        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = solutionDirectory };
        var response = await ProcessExecutor.StartProcessOnceAsync(info);

        return response.Output;
    }

    private async Task<ProcessOutput?> GitPull(CancellationToken cancellationToken)
    {
        const string fileName = "git";
        const string arguments = "pull";

        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = SolutionDirectory };
        var response = await ProcessExecutor.StartProcessOnceAsync(info, cancellationToken);

        return response;
    }
}