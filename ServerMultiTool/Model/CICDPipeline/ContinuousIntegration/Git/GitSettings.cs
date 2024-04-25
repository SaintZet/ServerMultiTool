using System;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;

[Serializable]
public struct GitSettings
{
    public bool Enable { get; set; }
    public bool ShouldPull { get; set; }
}