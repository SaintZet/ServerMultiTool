using System;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

[Serializable]
public class WebBrowserSettings
{
    public bool Enable { get; set; }
    public string? Url { get; set; }
}