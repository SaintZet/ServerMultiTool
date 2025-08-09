using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class ProcessExecutionOperationWrapper : PipelineOperationWrapper
{
    public override string Name { get; set; } = "IIS Reset";
    public override string Description { get; set; } = "Resets the IIS server to apply changes.";

    [ObservableProperty] string _fileName;
    [ObservableProperty] string _arguments;
    [ObservableProperty] int _retryCount;

    readonly ProcessExecutionOperation _operation;

    public ProcessExecutionOperationWrapper(ProcessExecutionOperation operation)
        : base(operation)
    {
        _operation = operation;

        RetryCount = operation.RetryCount;
        FileName = operation.FileName;
        Arguments = operation.Arguments;
    }

    public override PipelineOperation ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        _operation.UpdateRetryCount(RetryCount);

        return _operation;
    }
}