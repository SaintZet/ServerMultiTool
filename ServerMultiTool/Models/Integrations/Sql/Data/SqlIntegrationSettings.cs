using System;

namespace ServerMultiTool.Models.Integrations.Sql.Data;

[Serializable]
public struct SqlIntegrationSettings
{
    public bool Enable { get; set; }
    public string PathToSqlScript { get; set; }
    public string ConnectionString { get; set; }
}