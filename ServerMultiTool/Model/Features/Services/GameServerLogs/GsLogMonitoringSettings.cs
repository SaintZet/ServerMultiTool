using ServerMultiTool.Model.Common;
using System;
using System.Text.Json.Serialization;

namespace ServerMultiTool.Model.Features.Services.GameServerLogs;

[Serializable]
public class GsLogMonitoringSettings
{
    [JsonInclude] public bool Enable { get; set; }
    [JsonInclude] public DirectoryModel? MasterLogDirectory { get; set; }
    [JsonInclude] public DirectoryModel? SegmentLogDirectory { get; set; }
}