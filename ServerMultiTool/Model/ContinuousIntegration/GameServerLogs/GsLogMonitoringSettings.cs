using ServerMultiTool.Model.Common;
using System;

namespace ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;

[Serializable]
public class GsLogMonitoringSettings
{
    public bool Enable { get; set; }
    public DirectoryModel? MasterLogDirectory { get; set; }
    public DirectoryModel? SegmentLogDirectory { get; set; }
}