using System;

namespace ServerMultiTool.Models.Deployment.Data;

[Serializable]
public struct ProjectDeploySettings
{
    public string ProjectName { get; set; }
    public bool ShouldDeploy { get; set; }
    public bool OnlyBin { get; set; }
}