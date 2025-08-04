using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Common.Logs;
using ServerMultiTool.Model.Domain.Pipeline.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Domain.Pipeline;

public abstract class BasePipelineOperation : IPipelineOperation
{
    public Guid Guid { get; } = Guid.NewGuid();
    public bool Enabled { get; private set; } = true;
    public string Name { get; private set; }
    public string SolutionDirectory { get; private set; } = null!;
    public string HttpDirectory { get; private set; } = null!;

    protected Logger Logger { get; }

    protected BasePipelineOperation(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name), "Operation name cannot be null.");
        Logger = new Logger(Name);
    }

    protected abstract Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken);

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Enabled is false)
            throw new ArgumentException("Can`t execute disabled operation");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteOperationsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return PipelineOperationResult.Cancelled;
        }
    }

    public virtual void UpdateSolutionDirectory(DirectoryModel directory)
    {
        SolutionDirectory = directory.Path;
    }

    public virtual void UpdateHttpDirectory(DirectoryModel directory)
    {
        HttpDirectory = directory.Path;
    }

    public virtual void EnableOperation(bool enable)
    {
        Enabled = enable;
    }
}