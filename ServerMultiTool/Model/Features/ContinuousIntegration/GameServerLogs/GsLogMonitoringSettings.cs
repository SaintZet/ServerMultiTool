using ServerMultiTool.Model.Domain.Common;
using System;
using System.Text.Json.Serialization;

namespace ServerMultiTool.Model.Features.ContinuousIntegration.GameServerLogs;

[Serializable]
public class GsLogMonitoringSettings
{
    [JsonInclude] public bool Enable { get; set; }
    [JsonInclude] public DirectoryModel? MasterLogDirectory { get; set; }
    [JsonInclude] public DirectoryModel? SegmentLogDirectory { get; set; }
}