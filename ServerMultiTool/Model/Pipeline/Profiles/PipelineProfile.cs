using System;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.ContinuousIntegration.Git;

namespace ServerMultiTool.Model.Pipeline.Profiles;

[Serializable]
public class PipelineProfile
{
    public string Name { get; set; }
    public ProjectSettings[] SettingsPerProject { get; set; }
    public GitSettings GitSettings { get; set; }
    public InternetInformationSettings InternetInformationSettings { get; set; }
    public SqlExecutionSettings SqlExecutionSettings { get; set; }
    public WebBrowserSettings WebBrowserSettings { get; set; }
    public LogMonitoringSettings MonitorLogFilesSettings { get; set; }
    public HttpMonitoringSettings HttpMonitoringSettings { get; set; }
}