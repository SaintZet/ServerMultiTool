using ServerMultiTool.Model.Domain.Common.EventAggregator;
using ServerMultiTool.Model.Domain.Common.Logs;
using ServerMultiTool.Model.Features.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System.Collections.ObjectModel;
using System.Windows;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Management;

public class GsLogMonitor
{
    private readonly GsLogMonitoringService _masterLogService;
    private readonly GsLogMonitoringService _segmentLogService;

    public ObservableCollection<LogEvent> AppLogMessages { get; } = [];
    public ObservableCollection<LogEvent> MasterLogMessages { get; } = [];
    public ObservableCollection<LogEvent> SegmentLogMessages { get; } = [];

    public GsLogMonitor()
    {
        _masterLogService = new GsLogMonitoringService();
        _masterLogService.Subscribe<LogEvent>(AddNewMasterLogEvent);

        _segmentLogService = new GsLogMonitoringService();
        _segmentLogService.Subscribe<LogEvent>(AddNewSegmentLogEvent);

        GlobalEventAggregator.Instance.Subscribe<LogEvent>(AddNewGlobalLogEvent);
    }

    public void UpdateLogServices(PipelineProfileWrapper profile)
    {
        var settings = profile.ToOriginal().GsLogMonitoringSettings;

        _ = _masterLogService.UpdateSettings(settings.Enable, settings.MasterLogDirectory);
        _ = _segmentLogService.UpdateSettings(settings.Enable, settings.SegmentLogDirectory);

        Application.Current.Dispatcher.Invoke(() =>
        {
            MasterLogMessages.Clear();
            SegmentLogMessages.Clear();
        });
    }

    public void ClearAppLogs()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AppLogMessages.Clear();
        });
    }

    private void AddNewMasterLogEvent(LogEvent logEvent) =>
        AddNewLogEvent(logEvent, MasterLogMessages);

    private void AddNewSegmentLogEvent(LogEvent logEvent) =>
        AddNewLogEvent(logEvent, SegmentLogMessages);

    private void AddNewGlobalLogEvent(LogEvent logEvent) =>
        AddNewLogEvent(logEvent, AppLogMessages);

    private static void AddNewLogEvent(LogEvent logEvent, ObservableCollection<LogEvent> targetCollection)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (targetCollection.Contains(logEvent))
                return;

            targetCollection.Add(logEvent);
        });
    }
}