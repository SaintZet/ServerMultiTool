using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class ProcessExecutionOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "Process Execution";

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

    public override PipelineOperationBase ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        _operation.UpdateRetryCount(RetryCount);

        return _operation;
    }
}
