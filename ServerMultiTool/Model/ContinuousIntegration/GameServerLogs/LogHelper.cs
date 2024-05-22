using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ServerMultiTool.Model.Common.Logs;

namespace ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;

public static class LogHelper
{
    private static readonly Regex LogLineRegex = new(@"^(\d{2}:\d{2}:\d{2}\.\d{3})\s+(\w+)\s+\[(\d+)\]\s+(.+?)\s+-\s+(.*)$");

    public static IEnumerable<LogEvent> ParseLogLines(IEnumerable<string> logLines)
    {
        var result = new List<LogEvent>();
        
        foreach (var currentLine in logLines)
        {
            var currentLogEvent = ParseLogLine(currentLine);

            if (currentLogEvent is not null)
            {
                result.Add(currentLogEvent);
                continue;
            }

            if (currentLogEvent is null && result.Any()) 
                result.Last().Message.AddDetails(currentLine);
        }

        return result;
    }
    
    private static LogEvent? ParseLogLine(string logLine)
    {
        var match = LogLineRegex.Match(logLine);
        if (!match.Success)
            return null;

        var timestamp = ParseTimestamp(match.Groups[1].Value);
        var messageType = ParseMessageType(match.Groups[2].Value);
        var sender = match.Groups[4].Value;
        var message = match.Groups[5].Value;

        var logMessage = new LogMessage(message, messageType);

        return new LogEvent(timestamp, sender, logMessage);
    }

    private static DateTime ParseTimestamp(string timestamp) => 
        DateTime.Parse(timestamp);

    private static LogMessageType ParseMessageType(string messageType) => 
        Enum.TryParse(messageType, true, out LogMessageType result) ? result : LogMessageType.Info;
}
