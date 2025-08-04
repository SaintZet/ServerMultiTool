using ServerMultiTool.Model.Domain.Common;
using System;

namespace ServerMultiTool.Model.Features.ContinuousIntegration.GameServerLogs;

[Serializable]
public class GsLogMonitoringSettings
{
    public bool Enable { get; set; }
    public DirectoryModel? MasterLogDirectory { get; set; }
    public DirectoryModel? SegmentLogDirectory { get; set; }
}