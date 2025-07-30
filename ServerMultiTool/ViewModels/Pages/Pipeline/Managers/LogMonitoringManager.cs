using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.Pipeline.Profiles;
using System.Collections.ObjectModel;
using System.Windows;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Managers;

public class LogMonitoringManager
{
    private readonly LogMonitoringService _masterLogService;
    private readonly LogMonitoringService _segmentLogService;

    public ObservableCollection<LogEvent> AppLogMessages { get; } = [];
    public ObservableCollection<LogEvent> MasterLogMessages { get; } = [];
    public ObservableCollection<LogEvent> SegmentLogMessages { get; } = [];

    public LogMonitoringManager()
    {
        _masterLogService = new LogMonitoringService();
        _masterLogService.Subscribe<LogEvent>(AddNewMasterLogEvent);

        _segmentLogService = new LogMonitoringService();
        _segmentLogService.Subscribe<LogEvent>(AddNewSegmentLogEvent);

        GlobalEventAggregator.Instance.Subscribe<LogEvent>(AddNewGlobalLogEvent);
    }

    public void UpdateLogServices(PipelineProfile profile)
    {
        var settings = profile.MonitorLogFilesSettings;

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