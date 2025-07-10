using System;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

[Serializable]
public class HttpMonitoringSettings
{
    public bool Enable { get; set; }
    public bool PingSegment { get; set; }
    public bool PingMaster { get; set; }
    public double TimeoutMinutes { get; set; } = 5;
}