using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Infrastructure.Settings;

namespace ServerMultiTool.Model.Infrastructure.DefaultValues;

public static class DefaultAppSettings
{
    public static AppSettings GetDefaultAppSettings()
    {
        var solutionDirectories = new[] { new DirectoryModel { Name = "Raid", Path = @"C:\Raid" } };
        var httpDirectories = new[] { new DirectoryModel { Name = "HTTP Raid", Path = @"C:\HTTP\Raid" } };

        return new AppSettings
        {
            SolutionDirectories = solutionDirectories,
            CurrentSolutionDirectoryName = solutionDirectories[0].Name,
            HttpDirectories = httpDirectories,
            CurrentHttpDirectoryName = httpDirectories[0].Name,
            CurrentPipelineProfileName = "Standard Profile",
            Log4NetConfigPath = "log4net.config",
        };
    }
}