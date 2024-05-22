using System;
using System.Threading.Tasks;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Common.ProcessExecutor;

namespace ServerMultiTool.Model.Pipeline.Contracts;

public interface IPipelineOperation
{
    Task<OperationResult> ExecuteAsync();
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

    protected abstract Task<OperationResult> ExecuteOperationsAsync();

    public async Task<OperationResult> ExecuteAsync()
    {
        try
        {
            return await ExecuteOperationsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, ex.StackTrace);
            return OperationResult.Failure;
        }
    }

    public virtual void UpdateSolutionDirectory(DirectoryModel directory) => 
        SolutionDirectory = directory.Path;

    public virtual void UpdateHttpDirectory(DirectoryModel directory) => 
        HttpDirectory = directory.Path;
}