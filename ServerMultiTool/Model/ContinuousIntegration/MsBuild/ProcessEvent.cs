using System;

namespace ServerMultiTool.Model.ContinuousIntegration.MsBuild;

[Serializable]
public class ProcessEvent
{
    public required string Path { get; set; }
    public string? Arguments { get; set; }
}