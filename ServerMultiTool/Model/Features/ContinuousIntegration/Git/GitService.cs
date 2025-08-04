using ServerMultiTool.Model.Domain.Common.Logs;
using ServerMultiTool.Model.Domain.Common.ProcessExecutor;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousIntegration.Git;

public class GitService
{
    private readonly Logger _logger;
    private readonly ProcessExecutor _processExecutor;

    public GitService()
    {
        _logger = new Logger(GetType());
        _processExecutor = new ProcessExecutor(_logger);
    }

    public async Task<string?> GetCurrentBranchNameAsync(string workingDirectory, CancellationToken cancellationToken = default)
    {
        const string fileName = "git";
        const string arguments = "rev-parse --abbrev-ref HEAD";

        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = workingDirectory };
        var response = await _processExecutor.StartProcessOnceAsync(info, cancellationToken);

        return response.Output;
    }

    public async Task<ProcessOutput?> GitPullAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        const string fileName = "git";
        const string arguments = "pull";

        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = workingDirectory };
        var response = await _processExecutor.StartProcessOnceAsync(info, cancellationToken);

        return response;
    }
}