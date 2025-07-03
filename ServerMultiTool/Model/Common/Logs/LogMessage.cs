using System;

namespace ServerMultiTool.Model.Common.Logs;

public class LogMessage(LogMessageType type, string message)
{
    public LogMessageType Type { get; } = type;
    public string BaseMessage { get; } = message;
    public string ExtendedMessage { get; private set; } = string.Empty;

    public void AddDetails(string currentLine) => 
        ExtendedMessage += currentLine + Environment.NewLine;

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
            return false;

        var other = (LogMessage)obj;
        
        return BaseMessage == other.BaseMessage &&
               ExtendedMessage == other.ExtendedMessage &&
               Type == other.Type;
    }

    public override int GetHashCode() => 
        HashCode.Combine(BaseMessage, Type);
}