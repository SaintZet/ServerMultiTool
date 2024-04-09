using System;
using ServerMultiTool.Models.Build.Data;
using ServerMultiTool.Models.Deployment.Data;
using ServerMultiTool.Models.Integrations.Git.Data;
using ServerMultiTool.Models.Integrations.Sql.Data;
using ServerMultiTool.Models.Integrations.WebBrowser.Data;

namespace ServerMultiTool.Models.Settings.Profiles.Data;

[Serializable]
public struct SettingsProfile
{
    public int Id { get; set; }
    public BuildSettings BuildSettings { get; set; }
    public DeploySettings DeploySettings { get; set; }
    public GitIntegrationSettings GitIntegrationSettings { get; set; }
    public SqlIntegrationSettings SqlIntegrationSettings { get; set; }
    public WebBrowserSettings WebBrowserSettings { get; set; }
}