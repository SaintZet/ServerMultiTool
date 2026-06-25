using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
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

    private static readonly Regex TimeStampRegex = GetTimeStampRegex();

    private readonly Logger _logger;
    private readonly Timer _hourlyTimer;

    private CancellationTokenSource? _cancellationToken;

    private DateTime _lastHour = DateTime.MinValue;
    private string _cachedFileName = string.Empty;

    private bool _isEnabled;
    private DirectoryModel _monitoredDirectory = null!;

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
        if (_isEnabled)
            await StopMonitoringAsync();

        _isEnabled = enable;

        if (_isEnabled && logDirectory is not null)
            StartMonitoring(logDirectory);
    }

    private async Task OnTimerElapsed(object? sender, ElapsedEventArgs e)
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

    private void StartMonitoring(DirectoryModel directory)
    {
        if (directory.Path.IsNullOrEmpty())
            return;

        if (_cancellationToken is not null)
            return;

        _cancellationToken = new CancellationTokenSource();
        var token = _cancellationToken.Token;

        _monitoredDirectory = directory;

        _logger.LogInfo($"Start monitoring {directory.Name} logs at {directory.Path}");

        _ = Task.Run(async () => await MonitorLogDirectoryAsync(directory, _cancellationToken.Token), _cancellationToken.Token);
    }

    private async Task StopMonitoringAsync()
    {
        if (_cancellationToken is null)
            return;

        await _cancellationToken.CancelAsync();

        _cancellationToken.Dispose();
        _cancellationToken = null;

        _logger.LogInfo($"Stop monitoring {_monitoredDirectory.Name} logs at {_monitoredDirectory?.Path ?? "unknown directory"}");
    }

    private async Task MonitorLogDirectoryAsync(DirectoryModel directory, CancellationToken cancellationToken)
    {
        var startTime = DateTime.Now;

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            var path = GetActualLogFileName(directory);

            if (File.Exists(path) is false)
                continue;

            try
            {
                await ProcessLogFileAsync(path, startTime, cancellationToken);
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
        }
    }

    private async Task ProcessLogFileAsync(string path, DateTime startTime, CancellationToken cancellationToken)
    {
        await using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);

        await SkipToStartTimeAsync(reader, startTime, cancellationToken);

        var lines = await ReadRemainingLinesAsync(reader, cancellationToken);
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

    private static async Task SkipToStartTimeAsync(StreamReader reader, DateTime startTime, CancellationToken cancellationToken)
    {
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
                break;

            var timestamp = ExtractTimeStamp(line);
            if (timestamp >= startTime)
                break;
        }
    }

    private static DateTime? ExtractTimeStamp(string line)
    {
        var match = TimeStampRegex.Match(line);

        if (match.Success is false)
            return null;

        if (DateTime.TryParseExact(match.Value, "HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
            return timestamp;

        return null;
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

    [GeneratedRegex(@"\b\d{2}:\d{2}:\d{2}\.\d{3}\b")]
    private static partial Regex GetTimeStampRegex();
}
