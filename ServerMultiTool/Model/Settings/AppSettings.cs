using System;

namespace ServerMultiTool.Model.Settings;

[Serializable]
public struct AppSettings
{
    public string SolutionDirectory { get; set; }
    public string HttpDirectory { get; set; }
    public string Log4NetConfigPath { get; set; } 
}