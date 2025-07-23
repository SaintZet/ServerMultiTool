using System.Threading;
using System.Threading.Tasks;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Common.ProcessExecutor;

namespace ServerMultiTool.Model.Pipeline.Contracts;

public interface IPipelineOperation
{
    Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken);
    void UpdateSolutionDirectory(DirectoryModel directory);
    void UpdateHttpDirectory(DirectoryModel directory);
}

public abstract class PipelineOperation : IPipelineOperation
{
    protected readonly Logger Logger;
    protected readonly ProcessExecutor ProcessExecutor;

    protected string SolutionDirectory { get; private set; } = null!;
    protected string HttpDirectory { get; private set; } = null!;

    protected PipelineOperation()
    {
        Logger = new Logger(GetType());
        ProcessExecutor = new ProcessExecutor(Logger);
    }

    protected abstract Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken);

    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await ExecuteOperationsAsync(cancellationToken);
    }

    public virtual void UpdateSolutionDirectory(DirectoryModel directory) => 
        SolutionDirectory = directory.Path;

    public virtual void UpdateHttpDirectory(DirectoryModel directory) => 
        HttpDirectory = directory.Path;
}