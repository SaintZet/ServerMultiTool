using System;

namespace ServerMultiTool.Models.Integrations.Git.Data;

[Serializable]
public struct GitIntegrationSettings
{
    public bool Enable { get; set; }
    public bool ShouldPull { get; set; }
}