using System.IO;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;
using ServerMultiTool.Model.Settings;

namespace ServerMultiTool.Model;

internal static class DefaultValues
{
    public static AppSettings GetDefaultAppSettings()
    {
        var solutionDirectories = new[] { new DirectoryModel { Id = 1, Name = "Raid", Path =  @"C:\Raid" } };
        var httpDirectories = new[] { new DirectoryModel { Id = 2, Name = "HTTP Raid", Path =  @"C:\HTTP\Raid" } };
            
        return new AppSettings
        {
            SolutionDirectories = solutionDirectories,
            CurrentSolutionDirectoryId = solutionDirectories[0].Id,
            HttpDirectories = httpDirectories,
            CurrentHttpDirectoryId = httpDirectories[0].Id,
            Log4NetConfigPath = "log4net.config",
        };
    }

    public static PipelineProfile GetStandardProfile() => new()
    {
        Id = 0,
        Name = "Standard Profile",
        SettingsPerProject = new ProjectSettings[]
        {
            new()
            {
                ProjectName = "Master",
                ProjectPath = @"Server\Service.Master\Service.Master.csproj",
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
                ProjectName = "Segment",
                ProjectPath = @"Server\Service.Segment\Service.Segment.csproj",
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
    };
    
    public static PipelineProfile GetExtendedProfile(DirectoryModel solutionDirectory, DirectoryModel httpDirectory) => new()
    {
            Id = 1,
            Name = "Extended Profile",
            SettingsPerProject = new ProjectSettings[]
            {
                new()
                {
                    ProjectName = "Master",
                    ProjectPath = @"Server\Service.Master\Service.Master.csproj",
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
                    ProjectName = "Segment",
                    ProjectPath = @"Server\Service.Segment\Service.Segment.csproj",
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
                    ProjectName = "DataBlender",
                    ProjectPath = @"Utils\DataBlender\DataBlender.csproj",
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
        };
}