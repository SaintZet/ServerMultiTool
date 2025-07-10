using System;
using System.Collections.Generic;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.ContinuousIntegration.Git;

namespace ServerMultiTool.Model.Pipeline.Profiles;

[Serializable]
public class PipelineProfile
{
    public string Name { get; set; } = string.Empty;
    public List<ProjectSettings> SettingsPerProject { get; set; } = [];
    public GitSettings GitSettings { get; set; } = new();
    public InternetInformationSettings InternetInformationSettings { get; set; } = new();
    public SqlExecutionSettings SqlExecutionSettings { get; set; } = new();
    public WebBrowserSettings WebBrowserSettings { get; set; } = new();
    public LogMonitoringSettings MonitorLogFilesSettings { get; set; } = new();
    public HttpMonitoringSettings HttpMonitoringSettings { get; set; } = new();
}