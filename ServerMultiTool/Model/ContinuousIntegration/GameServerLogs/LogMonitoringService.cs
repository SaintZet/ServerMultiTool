using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using ServerMultiTool.Model.Common.EventAggregator;
using Timer = System.Timers.Timer;

namespace ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;

public class LogMonitoringService : BaseEventAggregator
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(LogMonitoringService));
    private static readonly Regex TimeStampRegex = new(@"\b\d{2}:\d{2}:\d{2}\.\d{3}\b");

    private readonly Timer _hourlyTimer;
    private CancellationTokenSource? _cancellationToken;

    private LogMonitoringSettings? _settings;

    public LogMonitoringService()
    {
        _hourlyTimer = new Timer();
        _hourlyTimer.Elapsed += async (sender, e) => await OnTimerElapsed(sender, e);
        _hourlyTimer.Interval = GetIntervalToNextHour();
        _hourlyTimer.Start();
    }

    public void UpdateMonitoringSettings(LogMonitoringSettings? settings)
    {
        _settings = settings;
        
        RestartMonitoringAsync(settings);
    }

    private async Task OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _hourlyTimer.Interval = GetIntervalToNextHour();
        
        await RestartMonitoringAsync(_settings);
    }

    private async Task RestartMonitoringAsync(LogMonitoringSettings? settings)
    {
        if (settings.Enable is false)
        {
            await StopMonitoringAsync();
            return;
        }
        
        await StopMonitoringAsync();
        await StartMonitoringAsync();
    }

    private static double GetIntervalToNextHour()
    {
        var currentTime = DateTime.Now;
        var nextHour = currentTime.AddHours(1).Date.AddHours(currentTime.Hour + 1);
        var millisecondsToNextHour = (nextHour - currentTime).TotalMilliseconds;
            
        return millisecondsToNextHour;
    }
    
    private async Task StartMonitoringAsync()
    {
        if (_cancellationToken is not null)
            return;
        
        try
        {
            Log.Info("Start monitoring");
            
            _cancellationToken = new CancellationTokenSource();
            await MonitorLogFileAsync(_cancellationToken.Token);
        }
        catch (OperationCanceledException)
        {
            Log.Error("Error when start monitoring");
        }
    }
    
    private static string GetCurrentFileName(LogMonitoringSettings? settings)
    {
        var fileName =  DateTime.Now.ToString(@"yyyy-MM-dd\\all.HH-00") + ".log";
        return Path.Combine(settings.LogFilesDirectories[0].Path, fileName);
    }

    private async Task StopMonitoringAsync()
    {
        if (_cancellationToken is null)
            return;
    
        await _cancellationToken.CancelAsync();

        _cancellationToken.Dispose();
        _cancellationToken = null;
        
        Log.Info("Stop monitoring");
    }
    
    private async Task MonitorLogFileAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.Now;
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            
            var path = GetCurrentFileName(_settings);
            
            if (File.Exists(path) is false)
                continue;

            try
            {
                await using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream);

                await SkipToStartTimeAsync(reader, startTime, cancellationToken);

                var lines = await ReadRemainingLinesAsync(reader, cancellationToken);

                var filteredLines = FilterAndShortenLines(lines);

                foreach (var logEvent in LogHelper.ParseLogLines(filteredLines)) 
                    Publish(logEvent);
            }
            catch (Exception ex)
            {
                Log.Error(ex.StackTrace);
            }
        }
    }

    private static async Task SkipToStartTimeAsync(StreamReader reader, DateTime startTime, CancellationToken cancellationToken)
    {
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null)
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
            if (line != null)
                lines.Add(line);
        }

        return lines;
    }

    private static IEnumerable<string> FilterAndShortenLines(IEnumerable<string> lines) =>
        lines
            .Where(line => line.Contains("ClusterSettings") is false)
            .Select(line => line.Length > 300 ? line[..300] + "..." : line);
}