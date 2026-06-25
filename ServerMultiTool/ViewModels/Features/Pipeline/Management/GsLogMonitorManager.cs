using System.Collections.ObjectModel;
using System.Windows;
using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Features.Services.GameServerLogs;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Management;

public class GsLogMonitorManager
{
    private readonly GsLogMonitoringService _masterLogService;
    private readonly GsLogMonitoringService _segmentLogService;
    private bool _hasAppliedSettings;
    private bool _lastEnable;
    private string? _lastMasterPath;
    private string? _lastSegmentPath;

    public ObservableCollection<LogEvent> AppLogMessages { get; } = [];
    public ObservableCollection<LogEvent> MasterLogMessages { get; } = [];
    public ObservableCollection<LogEvent> SegmentLogMessages { get; } = [];

    public GsLogMonitorManager()
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
        var masterPath = settings.MasterLogDirectory?.Path;
        var segmentPath = settings.SegmentLogDirectory?.Path;

        if (_hasAppliedSettings
            && _lastEnable == settings.Enable
            && string.Equals(_lastMasterPath, masterPath, System.StringComparison.OrdinalIgnoreCase)
            && string.Equals(_lastSegmentPath, segmentPath, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _hasAppliedSettings = true;
        _lastEnable = settings.Enable;
        _lastMasterPath = masterPath;
        _lastSegmentPath = segmentPath;

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
