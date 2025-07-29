using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class WebBrowserSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty]
    private bool _enable;

    [ObservableProperty]
    private string _url = string.Empty;

    public WebBrowserSettingsWrapper(WebBrowserSettings settings)
    {
        Enable = settings.Enable;
        Url = settings.Url ?? string.Empty;
    }

    public WebBrowserSettings ToWebBrowserSettings() => new()
    {
        Enable = Enable,
        Url = Url
    };
}