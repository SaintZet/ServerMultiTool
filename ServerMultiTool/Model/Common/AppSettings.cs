using System;

namespace ServerMultiTool.Model.Common;

[Serializable]
public class AppSettings
{
    public string CurrentSolutionDirectoryName { get; set; } = string.Empty;
    public DirectoryModel[] SolutionDirectories { get; set; } = [];

    public string CurrentHttpDirectoryName { get; set; } = string.Empty;
    public DirectoryModel[] HttpDirectories { get; set; } = [];

    public DirectoryModel[] LogDirectories { get; set; } = [];

    public string CurrentPipelineProfileName { get; set; } = string.Empty;

    public bool PipelineLogFilterInfoEnabled { get; set; } = true;
    public bool PipelineLogFilterSuccessEnabled { get; set; } = true;
    public bool PipelineLogFilterWarnEnabled { get; set; } = true;
    public bool PipelineLogFilterErrorEnabled { get; set; } = true;

    public string Log4NetConfigPath { get; set; } = string.Empty;

    // Auto-update settings
    public string? UpdateFeedUrl { get; set; }
    public string? UpdatePublicKey { get; set; }
    public bool CheckForUpdatesOnStartup { get; set; } = true;
}
