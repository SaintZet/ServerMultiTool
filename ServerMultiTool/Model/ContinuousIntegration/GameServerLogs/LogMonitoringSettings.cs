using System;
using ServerMultiTool.Model.Common;

namespace ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;

[Serializable]
public class LogMonitoringSettings
{
    public bool Enable { get; set; }
    public DirectoryModel? LogDirectory { get; set; }
}