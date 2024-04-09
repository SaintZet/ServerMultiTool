using System;
using System.Collections.Generic;

namespace ServerMultiTool.Models.Build.Data;

[Serializable]
public struct ProjectBuildSettings
{
    public string ProjectName { get; set; }
    public string ProjectPath { get; set; }
    public List<string> Parameters { get; set; }
    public bool ShouldBuild { get; set; }
}