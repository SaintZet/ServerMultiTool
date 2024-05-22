using System;

namespace ServerMultiTool.Model.ContinuousIntegration.Git;

[Serializable]
public class GitSettings
{
    public bool Enable { get; set; }
    public bool ShouldPull { get; set; }
}