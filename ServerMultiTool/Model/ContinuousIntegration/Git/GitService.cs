using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousIntegration.Git;

public class GitService : PipelineOperation
{
    private readonly GitSettings? _settings;

    public GitService() { }

    public GitService(GitSettings? settings) => 
        _settings = settings;

    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (_settings!.ShouldPull) 
            await GitPull(cancellationToken);
        
        return OperationResult.Success;
    }

    public async Task<string?> GetCurrentBranchName(string solutionDirectory)
    {
        const string fileName = "git";
        const string arguments = "rev-parse --abbrev-ref HEAD";
        
        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = solutionDirectory };
        var response = await ProcessExecutor.StartProcessOnceAsync(info);
        
        return response.Output;
    }

    private async Task<string?> GitPull(CancellationToken cancellationToken)
    {
        const string fileName = "git";
        const string arguments = "pull";
        
        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = SolutionDirectory };
        var response = await ProcessExecutor.StartProcessOnceAsync(info, cancellationToken);
        
        return response.Output;
    }
}