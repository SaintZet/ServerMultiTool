using System;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;

[Serializable]
public struct MsBuildSettings
{
    public bool Enable { get; set; }
    public string[] Parameters { get; set; }
    public ProcessEvent[] PreBuildEvents { get; set; }
    public ProcessEvent[] PostBuildEvents { get; set; }
}


[Serializable]
public class ProcessEvent
{
    public string Path { get; set; }
    public string Arguments { get; set; }
}