using System;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;

namespace ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

[Serializable]
public struct PipelineProfile
{
    public long Id { get; set; }
    public ProjectSettings[] SettingsPerProject { get; set; }
    public GitSettings GitSettings { get; set; }
    public SqlExecutionSettings SqlExecutionSettings { get; set; }
    public WebBrowserSettings WebBrowserSettings { get; set; }
}