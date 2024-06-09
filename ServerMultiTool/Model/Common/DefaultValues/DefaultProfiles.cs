using System.IO;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline.Profiles;

namespace ServerMultiTool.Model.Common.DefaultValues;

public static class DefaultProfiles
{
    public static PipelineProfile GetDevProfile() => new()
    {
        Name = "Dev Profile",
        SettingsPerProject = new ProjectSettings[]
        {
            new()
            {
                Project = new DirectoryModel
                {
                    Name = "Master", 
                    Path = @"Server\Service.Master\Service.Master.csproj"
                },
                MsBuildSettings = new MsBuildSettings
                {
                    Enable = false,
                    Parameters = new[]{"/p:Configuration=Debug", "/p:PreBuildEvent=", "/t:build"}
                },
                DeliverySettings = new DeliverySettings
                {
                    DeliveryBin = false,
                }
            },
            new()
            {
                Project = new DirectoryModel
                {
                    Name = "Segment", 
                    Path = @"Server\Service.Segment\Service.Segment.csproj"
                },
                MsBuildSettings = new MsBuildSettings
                {
                    Enable = false,
                    Parameters = new[]{"/p:Configuration=Debug", "/p:PreBuildEvent=", "/t:build"}
                },
                DeliverySettings = new DeliverySettings
                {
                    DeliveryBin = false,
                }
            },
        },
        GitSettings = new GitSettings
        {
            Enable = false,
            ShouldPull = false,
        },
        SqlExecutionSettings = new SqlExecutionSettings
        {
            Enable = false,
        },
        WebBrowserSettings = new WebBrowserSettings
        {
            Enable = false,
            Url = "http://localhost/Raid/Segment00/Segment.ashx",
        },
        InternetInformationSettings = new InternetInformationSettings()
        {
            Enable = true,
        },
        MonitorLogFilesSettings = new LogMonitoringSettings()
        {
            Enable = true,
            LogDirectory = new DirectoryModel { Name = "Dev Profile Master", Path = @"C:\HTTP\Raid\Master\log" },
        },
    };

    public static PipelineProfile GetStandardProfile() => new()
    {
        Name = "Standard Profile",
        SettingsPerProject = new ProjectSettings[]
        {
            new()
            {
                Project = new DirectoryModel
                {
                    Name = "Master", 
                    Path = @"Server\Service.Master\Service.Master.csproj"
                },
                MsBuildSettings = new MsBuildSettings
                {
                    Enable = true,
                    Parameters = new[]{"/p:Configuration=Debug", "/p:PreBuildEvent=", "/t:build"}
                },
                DeliverySettings = new DeliverySettings
                {
                    DeliveryBin = true,
                }
            },
            new()
            {
                Project = new DirectoryModel
                {
                    Name = "Segment", 
                    Path = @"Server\Service.Segment\Service.Segment.csproj"
                },
                MsBuildSettings = new MsBuildSettings
                {
                    Enable = true,
                    Parameters = new[]{"/p:Configuration=Debug", "/p:PreBuildEvent=", "/t:build"}
                },
                DeliverySettings = new DeliverySettings
                {
                    DeliveryBin = true,
                }
            },
        },
        GitSettings = new GitSettings
        {
            Enable = true,
            ShouldPull = true,
        },
        SqlExecutionSettings = new SqlExecutionSettings
        {
            Enable = true,
            ConnectionString = "Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True",
            PathToSqlScript = @"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql",
        },
        WebBrowserSettings = new WebBrowserSettings
        {
            Enable = true,
            Url = "http://localhost/Raid/Segment00/Segment.ashx",
        },
        InternetInformationSettings = new InternetInformationSettings()
        {
            Enable = true,
        },
        MonitorLogFilesSettings = new LogMonitoringSettings()
        {
            Enable = true,
            LogDirectory = new DirectoryModel { Name = "Standard Profile Master", Path = @"C:\HTTP\Raid\Master\log" },
        },
    };
    
    public static PipelineProfile GetExtendedProfile(DirectoryModel solutionDirectory, DirectoryModel httpDirectory) => new()
    {
        Name = "Extended Profile",
        SettingsPerProject = new ProjectSettings[]
        {
            new()
            {
                Project = new DirectoryModel
                {
                    Name = "Master", 
                    Path = @"Server\Service.Master\Service.Master.csproj"
                },
                MsBuildSettings = new MsBuildSettings
                {
                    Enable = true,
                    Parameters = new[]{"/p:Configuration=Debug", "/t:rebuild"},
                },
                DeliverySettings = new DeliverySettings
                {
                    DeliveryBin = true,
                }
            },
            new()
            {
                Project = new DirectoryModel
                {
                    Name = "Segment", 
                    Path = @"Server\Service.Segment\Service.Segment.csproj"
                },
                MsBuildSettings = new MsBuildSettings
                {
                    Enable = true,
                    Parameters = new[]{"/p:Configuration=Debug", "/t:rebuild"}
                },
                DeliverySettings = new DeliverySettings
                {
                    DeliveryBin = true,
                }
            },
            new()
            {
                Project = new DirectoryModel
                {
                    Name = "DataBlender", 
                    Path = @"Utils\DataBlender\DataBlender.csproj"
                },
                MsBuildSettings = new MsBuildSettings
                {
                    Enable = true,
                    Parameters = new[]{"/p:Configuration=Debug", "/t:rebuild"},
                    PostBuildEvents = new ProcessEvent[]
                    {
                        new()
                        {
                            Path = @"C:\Raid\Utils\DataBlender\bin\Debug\DataBlender.exe",
                            Arguments = "",
                        }
                    },
                },
                DeliverySettings = new DeliverySettings
                {
                    DeliveryBin = false,
                    DeliveryDirectory = new[]
                    {
                        new DeliveryDirectories
                        {
                            Source = Path.Combine(solutionDirectory.Path, @"Server\Service.Master\App_Data\Storage"), 
                            Destination = Path.Combine(httpDirectory.Path, @"Master\App_Data\Storage\"),
                        },
                    },
                }
            },
        },
        GitSettings = new GitSettings
        {
            Enable = true,
            ShouldPull = true,
        },
        SqlExecutionSettings = new SqlExecutionSettings
        {
            Enable = true,
            ConnectionString = "Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True",
            PathToSqlScript = @"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql",
        },
        WebBrowserSettings = new WebBrowserSettings
        {
            Enable = true,
            Url = "http://localhost/Raid/Segment00/Segment.ashx",
        },
        InternetInformationSettings = new InternetInformationSettings()
        {
            Enable = true,
        },
        MonitorLogFilesSettings = new LogMonitoringSettings
        {
            Enable = true,
            LogDirectory = new DirectoryModel { Name = "Extended Profile Master", Path = @"C:\HTTP\Raid\Master\log" },
        },
    };
}