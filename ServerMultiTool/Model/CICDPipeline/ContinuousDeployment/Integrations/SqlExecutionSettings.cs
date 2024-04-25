using System;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;

[Serializable]
public struct SqlExecutionSettings
{
    public bool Enable { get; set; }
    public string PathToSqlScript { get; set; }
    public string ConnectionString { get; set; }
}