using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.ContinuousIntegration.MsBuild;

[Serializable]
public class MsBuildSettings
{
    public bool Enable { get; set; }
    public List<string>? Parameters { get; set; }
    public List<ProcessEvent>? PreBuildEvents { get; set; }
    public List<ProcessEvent>? PostBuildEvents { get; set; }
}


[Serializable]
public record ProcessEvent
{
    public required string Path { get; set; }
    public string? Arguments { get; set; }
}