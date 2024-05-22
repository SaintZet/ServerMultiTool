using System;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Settings;

namespace ServerMultiTool.Model.Pipeline.Profiles;

[Serializable]
public class ProjectSettings
{
    public DirectoryModel Project { get; set; }
    public MsBuildSettings MsBuildSettings { get; set; }
    public DeliverySettings DeliverySettings { get; set; }
}