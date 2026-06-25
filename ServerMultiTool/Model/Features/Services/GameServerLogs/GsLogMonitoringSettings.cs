using System;
using System.Text.Json.Serialization;
using ServerMultiTool.Model.Common;

namespace ServerMultiTool.Model.Features.Services.GameServerLogs;

[Serializable]
public class GsLogMonitoringSettings
{
    [JsonInclude] public bool Enable { get; set; }
    [JsonInclude] public DirectoryModel? MasterLogDirectory { get; set; }
    [JsonInclude] public DirectoryModel? SegmentLogDirectory { get; set; }
}
