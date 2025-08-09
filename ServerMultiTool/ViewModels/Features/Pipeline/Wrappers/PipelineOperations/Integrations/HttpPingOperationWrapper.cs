using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Collections.ObjectModel;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class HttpPingOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "HTTP Ping";

    [ObservableProperty] double _timeoutInMinutes;
    [ObservableProperty] ObservableCollection<string> _urls = [];

    public HttpPingOperationWrapper(HttpPingOperation operation) : base(operation)
    {
        Urls = new ObservableCollection<string>(operation.Urls);
        TimeoutInMinutes = operation.TimeoutInMinutes;
    }
}