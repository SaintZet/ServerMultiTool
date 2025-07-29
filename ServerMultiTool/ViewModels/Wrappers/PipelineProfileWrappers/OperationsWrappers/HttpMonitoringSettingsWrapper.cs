using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class HttpMonitoringSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty]
    private bool _enable;

    [ObservableProperty]
    private bool _pingSegment;

    [ObservableProperty]
    private bool _pingMaster;

    [ObservableProperty]
    private double _timeoutMinutes;

    public HttpMonitoringSettingsWrapper(HttpMonitoringSettings settings)
    {
        Enable = settings.Enable;
        PingSegment = settings.PingSegment;
        PingMaster = settings.PingMaster;
        TimeoutMinutes = settings.TimeoutMinutes;
    }

    public HttpMonitoringSettings ToHttpMonitoringSettings() => new()
    {
        Enable = Enable,
        PingSegment = PingSegment,
        PingMaster = PingMaster,
        TimeoutMinutes = TimeoutMinutes
    };
}