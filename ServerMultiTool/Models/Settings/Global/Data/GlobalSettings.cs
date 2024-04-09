using System;

namespace ServerMultiTool.Models.Settings.Global.Data;

[Serializable]
public struct GlobalSettings
{
    public int ProfileSettingId { get; set; }
    public string SolutionDirectory { get; set; }
    public string HttpDirectory { get; set; }
}