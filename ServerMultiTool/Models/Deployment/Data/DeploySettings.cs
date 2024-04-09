using System;
using System.Collections.Generic;

namespace ServerMultiTool.Models.Deployment.Data;

[Serializable]
public struct DeploySettings
{
    public bool Enable { get; set; }
    public List<ProjectDeploySettings> DeploySettingsPerProject { get; set; }
}