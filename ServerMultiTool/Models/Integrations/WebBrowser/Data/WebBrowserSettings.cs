using System;

namespace ServerMultiTool.Models.Integrations.WebBrowser.Data;

[Serializable]
public struct WebBrowserSettings
{
    public bool Enable { get; set; }
    public string Url { get; set; }
}