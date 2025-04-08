using System;

namespace ServerMultiTool.Model.Common.Logs;

public class LogEvent
{
    public DateTime Timestamp { get; }
    public string? Sender { get; }
    public LogMessage  Message { get; }

    public LogEvent(DateTime timestamp, string? sender, LogMessage message)
    {
        Timestamp = timestamp;
        Sender = sender;
        Message = message;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
            return false;

        var other = (LogEvent)obj;
        
        return Timestamp == other.Timestamp &&
               Sender == other.Sender &&
               Message.Equals(other.Message);
    }

    public override int GetHashCode() => 
        HashCode.Combine(Timestamp, Sender, Message);
}