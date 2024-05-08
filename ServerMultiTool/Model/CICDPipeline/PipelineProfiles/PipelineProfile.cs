using System;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;
using ServerMultiTool.Model.Settings;

namespace ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

[Serializable]
public struct PipelineProfile
{
    public string Name { get; set; }
    public ProjectSettings[] SettingsPerProject { get; set; }
    public GitSettings GitSettings { get; set; }
    public SqlExecutionSettings SqlExecutionSettings { get; set; }
    public WebBrowserSettings WebBrowserSettings { get; set; }
    public MonitorLogFilesSettings? MonitorLogFilesSettings { get; set; }
}

public class MonitorLogFilesSettings
{
    public bool Enable { get; set; }
    public DirectoryModel[] LogFilesDirectories { get; set; }
}