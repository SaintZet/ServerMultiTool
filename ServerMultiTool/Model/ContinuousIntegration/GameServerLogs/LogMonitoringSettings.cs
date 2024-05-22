using System;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Settings;

namespace ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;

[Serializable]
public class LogMonitoringSettings
{
    public bool Enable { get; set; }
    public DirectoryModel[] LogFilesDirectories { get; set; }
}