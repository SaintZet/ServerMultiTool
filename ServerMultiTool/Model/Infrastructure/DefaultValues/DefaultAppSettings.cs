using ServerMultiTool.Model.Common;

namespace ServerMultiTool.Model.Infrastructure.DefaultValues;

public static class DefaultAppSettings
{
    public static AppSettings GetDefaultAppSettings()
    {
        var solutionDirectories = new[] { new DirectoryModel { Name = "Raid", Path = @"C:\Raid" } };
        var httpDirectories = new[] { new DirectoryModel { Name = "HTTP Raid", Path = @"C:\HTTP\Raid" } };
        var logDirectories = new[]
        {
            new DirectoryModel { Name = "Master Logs", Path = @"C:\HTTP\Raid\Master\log" },
            new DirectoryModel { Name = "Segment Logs", Path = @"C:\HTTP\Raid\Segment00\log" }
        };

        return new AppSettings
        {
            SolutionDirectories = solutionDirectories,
            CurrentSolutionDirectoryName = solutionDirectories[0].Name,
            HttpDirectories = httpDirectories,
            CurrentHttpDirectoryName = httpDirectories[0].Name,
            LogDirectories = logDirectories,
            CurrentPipelineProfileName = "Standard Profile",
            Log4NetConfigPath = "log4net.config",
        };
    }
}
