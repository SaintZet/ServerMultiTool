using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;

namespace ServerMultiTool.Model.Features.Pipeline.Operations.Base;

[Serializable]
public abstract class PipelineOperationBase
{
    [JsonInclude] public abstract OperationType OperationType { get; }
    [JsonInclude] public Guid Guid { get; } = Guid.NewGuid();
    [JsonInclude] public bool Enabled { get; private set; } = true;
    [JsonInclude] public string Name { get; private set; }
    [JsonInclude] public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Если true — при провале этой операции весь шаг считается проваленным.
    /// </summary>
    [JsonInclude] public bool FailStepOnFailure { get; private set; } = true;

    [JsonIgnore] public string SolutionDirectory { get; private set; } = null!;
    [JsonIgnore] public string HttpDirectory { get; private set; } = null!;

    protected Logger Logger { get; }

    protected PipelineOperationBase(string name)
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

    public void SetFailStepOnFailure(bool value)
    {
        FailStepOnFailure = value;
    }

    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
    }
}
