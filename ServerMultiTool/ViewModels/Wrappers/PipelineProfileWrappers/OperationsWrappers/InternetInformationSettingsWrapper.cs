using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class InternetInformationSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty] private bool _enable;

    public InternetInformationSettingsWrapper(InternetInformationSettings settings)
    {
        Enable = settings.Enable;
    }

    public InternetInformationSettings ToInternetInformationSettings()
    {
        return new InternetInformationSettings
        {
            Enable = Enable
        };
    }

    partial void OnEnableChanged(bool value)
    {
        OnPropertyChanged(string.Empty);
    }
}