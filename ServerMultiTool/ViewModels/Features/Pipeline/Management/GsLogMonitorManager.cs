using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Features.Services.GameServerLogs;
using ServerMultiTool.Model.Infrastructure.DefaultValues;
using ServerMultiTool.Model.Infrastructure.Interfaces;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Management;

public class GsLogMonitorManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly Dictionary<string, LogMonitorEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private Predicate<object>? _filter;

    private readonly PipelineLogTabViewModel _appLogTab = new("App Log");

    public ObservableCollection<PipelineLogTabViewModel> LogTabs { get; } = [];

    public ObservableCollection<LogEvent> AppLogMessages => _appLogTab.Messages;

    public GsLogMonitorManager(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;

        LogTabs.Add(_appLogTab);

        GlobalEventAggregator.Instance.Subscribe<LogEvent>(AddNewGlobalLogEvent);
        _appSettingsService.Changed += OnAppSettingsChanged;

        ReloadLogTabs(_appSettingsService.Get());
    }

    public void SetFilter(Predicate<object> filter)
    {
        _filter = filter;
        ApplyFilterToAllTabs();
    }

    public void RefreshLogViews()
    {
        foreach (var tab in LogTabs)
            tab.LogView.Refresh();
    }

    private void OnAppSettingsChanged(object? sender, AppSettings settings)
    {
        Application.Current.Dispatcher.Invoke(() => ReloadLogTabs(settings));
    }

    private void ReloadLogTabs(AppSettings settings)
    {
        var logDirectories = settings.LogDirectories.Length > 0
            ? settings.LogDirectories
            : DefaultAppSettings.GetDefaultAppSettings().LogDirectories;

        var desiredPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var orderedTabs = new List<PipelineLogTabViewModel> { _appLogTab };

        foreach (var directory in logDirectories)
        {
            if (string.IsNullOrWhiteSpace(directory.Path))
                continue;

            var path = directory.Path.Trim();
            if (desiredPaths.Add(path) is false)
                continue;

            var header = string.IsNullOrWhiteSpace(directory.Name) ? path : directory.Name;
            var entry = GetOrCreateEntry(path, header);

            entry.Tab.Header = header;
            entry.Tab.Path = path;

            orderedTabs.Add(entry.Tab);

            _ = entry.Service.UpdateSettings(true, directory);
            entry.Tab.Messages.Clear();
        }

        foreach (var removedKey in _entries.Keys.Where(key => desiredPaths.Contains(key) is false).ToList())
        {
            var entry = _entries[removedKey];
            _ = entry.Service.UpdateSettings(false, null);
            _entries.Remove(removedKey);
        }

        LogTabs.Clear();
        foreach (var tab in orderedTabs)
            LogTabs.Add(tab);

        ApplyFilterToAllTabs();
    }

    public void ClearAppLogs()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _appLogTab.Messages.Clear();
        });
    }

    private LogMonitorEntry GetOrCreateEntry(string path, string header)
    {
        if (_entries.TryGetValue(path, out var existing))
            return existing;

        var tab = new PipelineLogTabViewModel(header, path);
        var service = new GsLogMonitoringService();
        service.Subscribe<LogEvent>(logEvent => AddNewLogEvent(logEvent, tab.Messages));

        var entry = new LogMonitorEntry(tab, service);
        _entries[path] = entry;
        return entry;
    }

    private void ApplyFilterToAllTabs()
    {
        foreach (var tab in LogTabs)
        {
            tab.LogView.Filter = _filter;
            tab.LogView.Refresh();
        }
    }

    private void AddNewGlobalLogEvent(LogEvent logEvent) =>
        AddNewLogEvent(logEvent, _appLogTab.Messages);

    private static void AddNewLogEvent(LogEvent logEvent, ObservableCollection<LogEvent> targetCollection)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (targetCollection.Contains(logEvent))
                return;

            targetCollection.Add(logEvent);
        });
    }

    private sealed record LogMonitorEntry(PipelineLogTabViewModel Tab, GsLogMonitoringService Service);
}
