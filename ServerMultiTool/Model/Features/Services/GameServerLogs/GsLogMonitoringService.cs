using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using Timer = System.Timers.Timer;

namespace ServerMultiTool.Model.Features.Services.GameServerLogs;

public partial class GsLogMonitoringService : BaseEventAggregator
{
    private const string LogFileNameFormat = @"yyyy-MM-dd\\all.HH-00";
    private const string LogFileExtension = ".log";

    private readonly Logger _logger;
    private readonly Timer _hourlyTimer;
    private readonly SemaphoreSlim _settingsUpdateGate = new(1, 1);

    private CancellationTokenSource? _cancellationToken;

    private DateTime _lastHour = DateTime.MinValue;
    private string _cachedFileName = string.Empty;
    private string _lastProcessedPath = string.Empty;
    private long _lastReadPosition;

    private bool _isEnabled;
    private DirectoryModel? _monitoredDirectory;

    public GsLogMonitoringService()
    {
        _logger = new Logger(GetType());

        _hourlyTimer = new Timer();
        _hourlyTimer.Elapsed += (sender, e) =>
        {
            _ = Task.Run(async () => await OnTimerElapsed(sender, e));
        };
        _hourlyTimer.Interval = GetIntervalToNextHour();
        _hourlyTimer.Start();
    }

    public async Task UpdateSettings(bool enable, DirectoryModel? logDirectory)
    {
        await _settingsUpdateGate.WaitAsync();
        try
        {
            var sameDirectory = AreSameDirectory(_monitoredDirectory, logDirectory);
            var sameSettings = _isEnabled == enable && (_isEnabled is false || sameDirectory);
            if (sameSettings)
                return;

            if (_isEnabled)
                await StopMonitoringAsync();

            _isEnabled = enable;

            if (_isEnabled && logDirectory is not null)
                StartMonitoring(logDirectory);
        }
        finally
        {
            _settingsUpdateGate.Release();
        }
    }

    private async Task OnTimerElapsed(object? _, ElapsedEventArgs __)
    {
        _hourlyTimer.Interval = GetIntervalToNextHour();

        if (_isEnabled is false || _monitoredDirectory is null)
            return;

        await StopMonitoringAsync();
        StartMonitoring(_monitoredDirectory);
    }

    private static double GetIntervalToNextHour()
    {
        var now = DateTime.Now;
        var nextHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
        return (nextHour - now).TotalMilliseconds;
    }

    private static bool AreSameDirectory(DirectoryModel? current, DirectoryModel? next)
    {
        if (current is null && next is null)
            return true;

        if (current is null || next is null)
            return false;

        return string.Equals(current.Path, next.Path, StringComparison.OrdinalIgnoreCase);
    }

    private void StartMonitoring(DirectoryModel directory)
    {
        if (directory.Path.IsNullOrEmpty())
            return;

        if (_cancellationToken is not null)
            return;

        _cancellationToken = new CancellationTokenSource();
        _monitoredDirectory = directory;
        _lastProcessedPath = string.Empty;
        _lastReadPosition = 0;

        _logger.LogInfo($"Start monitoring {directory.Name} at {directory.Path}");

        _ = Task.Run(async () => await MonitorLogDirectoryAsync(directory, _cancellationToken.Token), _cancellationToken.Token);
    }

    private async Task StopMonitoringAsync()
    {
        if (_cancellationToken is null)
            return;

        await _cancellationToken.CancelAsync();

        _cancellationToken.Dispose();
        _cancellationToken = null;

        _logger.LogInfo($"Stop monitoring {_monitoredDirectory?.Name ?? "Unknown"} at {_monitoredDirectory?.Path ?? "unknown directory"}");
    }

    private async Task MonitorLogDirectoryAsync(DirectoryModel directory, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var path = GetActualLogFileName(directory);

            if (File.Exists(path) is false)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                continue;
            }

            try
            {
                await ProcessLogFileAsync(path, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogExceptionWithPublish(ex);
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    private async Task ProcessLogFileAsync(string path, CancellationToken cancellationToken)
    {
        await using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        if (!string.Equals(_lastProcessedPath, path, StringComparison.OrdinalIgnoreCase))
        {
            _lastProcessedPath = path;
            _lastReadPosition = 0;
        }

        if (_lastReadPosition > fileStream.Length)
            _lastReadPosition = 0;

        fileStream.Seek(_lastReadPosition, SeekOrigin.Begin);
        using var reader = new StreamReader(fileStream);

        var lines = await ReadRemainingLinesAsync(reader, cancellationToken);
        _lastReadPosition = fileStream.Position;

        if (lines.Count == 0)
            return;

        var filteredLines = FilterAndShortenLines(lines);

        foreach (var logEvent in GsLogMonitoringUtils.ParseLogLines(filteredLines))
            Publish(logEvent);
    }

    private string GetActualLogFileName(DirectoryModel directory)
    {
        var now = DateTime.Now;

        if (now.Hour != _lastHour.Hour || now.Day != _lastHour.Day || string.IsNullOrEmpty(_cachedFileName))
        {
            _cachedFileName = now.ToString(LogFileNameFormat) + LogFileExtension;
            _lastHour = now;
        }

        return Path.Combine(directory.Path, _cachedFileName);
    }

    private static async Task<List<string>> ReadRemainingLinesAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var lines = new List<string>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is not null)
                lines.Add(line);
        }

        return lines;
    }

    private static IEnumerable<string> FilterAndShortenLines(IEnumerable<string> lines)
    {
        const string filterString = "ClusterSettings";
        const int maxLength = 300;

        foreach (var line in lines)
        {
            if (line.Contains(filterString, StringComparison.Ordinal))
                continue;

            yield return line.Length > maxLength ? line[..maxLength] + "..." : line;
        }
    }
}
