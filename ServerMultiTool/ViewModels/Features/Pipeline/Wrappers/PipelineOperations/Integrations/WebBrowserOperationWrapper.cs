using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.Pipeline.Operations.Network;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Collections.ObjectModel;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class WebBrowserOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "Web Browser Operation";
    [ObservableProperty] ObservableCollection<string> _urls = [];

    public WebBrowserOperationWrapper(WebBrowserOperation operation) : base(operation)
    {
        Urls = new ObservableCollection<string>(operation.Urls);
    }
}