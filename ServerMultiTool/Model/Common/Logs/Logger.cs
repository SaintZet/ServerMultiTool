using log4net;
using ServerMultiTool.Model.Common.EventAggregator;
using System;
using System.Reflection;

namespace ServerMultiTool.Model.Common.Logs;

public class Logger
{
    private readonly ILog _log4NetLogger;
    private readonly string? _derivedTypeName;

    public Logger(string serviceName)
    {
        _derivedTypeName = serviceName;
        _log4NetLogger = LogManager.GetLogger(serviceName);
    }

    public Logger(MemberInfo serviceType)
    {
        var serviceName = serviceType.Name;

        _derivedTypeName = serviceName;
        _log4NetLogger = LogManager.GetLogger(serviceName);
    }

    public void LogInfo(string message, string? details = null) =>
        _log4NetLogger.Info(GetDefaultFormattedLogInfo(message, details));

    public void LogInfoWithPublish(string message, string? details = null)
    {
        LogInfo(message, details);
        PublishLogEvent(LogMessageType.Info, message, details);
    }

    public void LogSuccess(string message, string? details = null) =>
        _log4NetLogger.Info(GetDefaultFormattedLogInfo(message, details));

    public void LogSuccessWithPublish(string message, string? details = null)
    {
        LogSuccess(message, details);
        PublishLogEvent(LogMessageType.Success, message, details);
    }

    public void LogWarn(string message, string? details = null) =>
        _log4NetLogger.Warn(GetDefaultFormattedLogInfo(message, details));

    public void LogWarnWithPublish(string message, string? details = null)
    {
        LogWarn(message, details);
        PublishLogEvent(LogMessageType.Warn, message, details);
    }

    public void LogError(string message, string? details = null) =>
        _log4NetLogger.Error(GetDefaultFormattedLogInfo(message, details));

    public void LogErrorWithPublish(string message, string? details = null)
    {
        LogError(message, details);
        PublishLogEvent(LogMessageType.Error, message, details);
    }

    public void LogException(Exception exception) =>
        LogError(exception.Message, exception.StackTrace);

    public void LogExceptionWithPublish(Exception exception)
    {
        LogException(exception);
        PublishLogEvent(LogMessageType.Exception, exception.Message, exception.StackTrace);
    }

    private void PublishLogEvent(LogMessageType type, string message, string? details = null)
    {
        var logMessage = new LogMessage(type, message);

        if (details is not null)
            logMessage.AddDetails(details);

        var logEvent = new LogEvent(DateTime.Now, _derivedTypeName, logMessage);

        GlobalEventAggregator.Instance.Publish(logEvent);
    }

    private static string GetDefaultFormattedLogInfo(string message, string? details) =>
        details is null ? message : message + Environment.NewLine + details;
}