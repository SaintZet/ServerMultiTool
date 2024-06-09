using System;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;

namespace ServerMultiTool.Model.Common;

[Serializable]
public class ProjectSettings
{
    public DirectoryModel Project { get; set; }
    public MsBuildSettings MsBuildSettings { get; set; }
    public DeliverySettings DeliverySettings { get; set; }
}