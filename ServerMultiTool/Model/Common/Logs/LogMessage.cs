using System;

namespace ServerMultiTool.Model.Common.Logs;

public class LogMessage
{
    public LogMessageType Type { get; }
    public string BaseMessage { get; }
    public string ExtendedMessage { get; private set; } 
    
    public LogMessage(LogMessageType type, string message)
    {
        Type = type;
        BaseMessage = message;
        ExtendedMessage = string.Empty;
    }

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