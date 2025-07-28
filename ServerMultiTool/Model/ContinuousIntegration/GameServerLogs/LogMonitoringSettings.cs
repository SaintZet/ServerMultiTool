using ServerMultiTool.Model.Common;
using System;

namespace ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;

[Serializable]
public class LogMonitoringSettings
{
    public bool Enable { get; set; }
    public DirectoryModel? MasterLogDirectory { get; set; }
    public DirectoryModel? SegmentLogDirectory { get; set; }
}