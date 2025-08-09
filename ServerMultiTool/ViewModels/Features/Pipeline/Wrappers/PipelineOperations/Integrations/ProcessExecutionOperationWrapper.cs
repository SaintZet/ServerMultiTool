using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class ProcessExecutionOperationWrapper : PipelineOperationWrapper
{
    [ObservableProperty] int _retryCount;

    public override string Name => "IIS Reset";
    public override string Description => "Resets the IIS server to apply changes.";

    readonly ProcessExecutionOperation _operation;

    public ProcessExecutionOperationWrapper(ProcessExecutionOperation operation)
        : base(operation)
    {
        _operation = operation;

        RetryCount = operation.RetryCount;
    }

    public override PipelineOperation ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        _operation.UpdateRetryCount(RetryCount);

        return _operation;
    }
}