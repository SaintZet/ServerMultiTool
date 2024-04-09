using System;
using System.Collections.Generic;

namespace ServerMultiTool.Models.Build.Data;

[Serializable]
public struct BuildSettings
{
    public bool Enable { get; set; }
    public List<ProjectBuildSettings> BuildSettingsPerProject { get; set; }
}