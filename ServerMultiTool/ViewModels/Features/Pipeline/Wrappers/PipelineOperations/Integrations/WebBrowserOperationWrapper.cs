using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Collections.ObjectModel;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class WebBrowserOperationWrapper : PipelineOperationWrapper
{
    public override string Name { get; set; } = "Web Browser Operation";
    public override string Description { get; set; } = "This operation opens a web browser to a specified URL.";

    [ObservableProperty] ObservableCollection<string> _urls = [];

    public WebBrowserOperationWrapper(WebBrowserOperation operation) : base(operation)
    {
        Urls = new ObservableCollection<string>(operation.Urls);
    }
}