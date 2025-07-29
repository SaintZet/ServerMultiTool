using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class InternetInformationSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty]
    private bool _enable;

    [ObservableProperty]
    private int _retryCount;

    public InternetInformationSettingsWrapper(InternetInformationSettings settings)
    {
        Enable = settings.Enable;
        RetryCount = settings.RetryCount;
    }

    public InternetInformationSettings ToInternetInformationSettings() => new()
    {
        Enable = Enable,
        RetryCount = RetryCount,
    };
}