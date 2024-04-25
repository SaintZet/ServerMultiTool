using System;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;

[Serializable]
public struct WebBrowserSettings
{
    public bool Enable { get; set; }
    public string Url { get; set; }
}