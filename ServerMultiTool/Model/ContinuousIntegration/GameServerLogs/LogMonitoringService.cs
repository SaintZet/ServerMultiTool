using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using Timer = System.Timers.Timer;

namespace ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;

public class LogMonitoringService : BaseEventAggregator
{
    private const string LogFileNameFormat = @"yyyy-MM-dd\\all.HH-00";
    private const string LogFileExtension = ".log";
    
    private static readonly Regex TimeStampRegex = new(@"\b\d{2}:\d{2}:\d{2}\.\d{3}\b");

    private readonly Logger _logger;
    private readonly Timer _hourlyTimer;
    
    private CancellationTokenSource? _cancellationToken;
    private LogMonitoringSettings? _settings;

    public LogMonitoringService()
    {
        _logger = new Logger(GetType());
        
        _hourlyTimer = new Timer();
        _hourlyTimer.Elapsed += async (sender, e) => await OnTimerElapsed(sender, e);
        _hourlyTimer.Interval = GetIntervalToNextHour();
        _hourlyTimer.Start();
    }

    public async Task UpdateSettings(LogMonitoringSettings settings)
    {
        var newSettings = settings ?? throw new ArgumentNullException(nameof(settings));

        if (_settings is { Enable: true })
            await StopMonitoringAsync();
        
        if (newSettings.Enable) 
            await StartMonitoringAsync(newSettings.LogDirectory);
        
        _settings = newSettings;
    }

    private async Task OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _hourlyTimer.Interval = GetIntervalToNextHour();

        if (_settings is null)
            return;
        
        await StopMonitoringAsync();
        await StartMonitoringAsync(_settings.LogDirectory);
    }

    private static double GetIntervalToNextHour()
    {
        var currentTime = DateTime.Now;
        var nextHour = currentTime.AddHours(1).Date.AddHours(currentTime.Hour + 1);
        var millisecondsToNextHour = (nextHour - currentTime).TotalMilliseconds;
            
        return millisecondsToNextHour;
    }
    
    private async Task StartMonitoringAsync(DirectoryModel directory)
    {
        if (_cancellationToken is not null)
            return;

        if (directory.Path.IsNullOrEmpty())
            return;
        
        _logger.LogInfo($"Start monitoring {directory.Name}");
        
        _cancellationToken = new CancellationTokenSource();
        await MonitorLogDirectoryAsync(_cancellationToken.Token, directory);
    }

    private async Task StopMonitoringAsync()
    {
        if (_cancellationToken is null)
            return;
    
        await _cancellationToken.CancelAsync();

        _cancellationToken.Dispose();
        _cancellationToken = null;
        
        _logger.LogInfo($"Stop monitoring {_settings!.LogDirectory.Name}");
    }
    
    private async Task MonitorLogDirectoryAsync(CancellationToken cancellationToken, DirectoryModel directory)
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

        foreach (var logEvent in LogMonitoringUtils.ParseLogLines(filteredLines)) 
            Publish(logEvent);
    }

    private static string GetActualLogFileName(DirectoryModel directory)
    {
        var fileName = DateTime.Now.ToString(LogFileNameFormat) + LogFileExtension;
        return Path.Combine(directory.Path, fileName);
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

    private static IEnumerable<string> FilterAndShortenLines(IEnumerable<string> lines) =>
        lines
            .Where(line => line.Contains("ClusterSettings") is false)
            .Select(line => line.Length > 300 ? line[..300] + "..." : line);
}