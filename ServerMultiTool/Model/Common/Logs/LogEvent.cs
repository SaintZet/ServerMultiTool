using System;

namespace ServerMultiTool.Model.Common.Logs;

public class LogEvent(DateTime timestamp, string? sender, LogMessage message)
{
    public DateTime Timestamp { get; } = timestamp;
    public string? Sender { get; } = sender;
    public LogMessage Message { get; } = message;

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
