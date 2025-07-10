using System;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;

namespace ServerMultiTool.Model.Common;

[Serializable]
public class ProjectSettings
{
    public DirectoryModel Project { get; set; } = new();
    public MsBuildSettings MsBuildSettings { get; set; } = new();
    public DeliverySettings DeliverySettings { get; set; } = new();
}