using System;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;

namespace ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

[Serializable]
public struct ProjectSettings
{
    public string ProjectName { get; set; }
    public string ProjectPath { get; set; }
    public MsBuildSettings MsBuildSettings { get; set; }
    public DeliverySettings DeliverySettings { get; set; }
}