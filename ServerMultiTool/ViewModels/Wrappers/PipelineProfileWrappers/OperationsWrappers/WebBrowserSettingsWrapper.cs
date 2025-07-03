using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class WebBrowserSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty] private bool _enable;
    [ObservableProperty] private string _url = string.Empty;

    public WebBrowserSettingsWrapper(WebBrowserSettings settings)
    {
        Enable = settings.Enable;
        Url = settings.Url ?? string.Empty;
    }

    public WebBrowserSettings ToWebBrowserSettings()
    {
        return new WebBrowserSettings
        {
            Enable = Enable,
            Url = Url
        };
    }

    partial void OnEnableChanged(bool value)
    {
        OnPropertyChanged(string.Empty);
    }

    partial void OnUrlChanged(string value)
    {
        OnPropertyChanged(string.Empty);
    }
}