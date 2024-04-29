using System;

namespace ServerMultiTool.Model.Settings;

[Serializable]
public struct AppSettings
{
    public long CurrentSolutionDirectoryId { get; set; }
    public DirectoryModel[] SolutionDirectories { get; set; }
    public long CurrentHttpDirectoryId { get; set; }
    public DirectoryModel[] HttpDirectories { get; set; }
    public string Log4NetConfigPath { get; set; } 
}