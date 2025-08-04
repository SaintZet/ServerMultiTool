using ServerMultiTool.Model.Domain.Common;
using System;

namespace ServerMultiTool.Model.Infrastructure.Settings;

[Serializable]
public class AppSettings
{
    public string CurrentSolutionDirectoryName { get; set; }
    public DirectoryModel[] SolutionDirectories { get; set; }

    public string CurrentHttpDirectoryName { get; set; }
    public DirectoryModel[] HttpDirectories { get; set; }

    public string CurrentPipelineProfileName { get; set; }

    public string Log4NetConfigPath { get; set; }
}