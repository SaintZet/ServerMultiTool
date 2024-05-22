using System;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

[Serializable]
public class SqlExecutionSettings
{
    public bool Enable { get; set; }
    public string PathToSqlScript { get; set; }
    public string ConnectionString { get; set; }
}