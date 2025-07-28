using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline.Profiles;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServerMultiTool.Model.Common.DefaultValues;

public static class DefaultProfiles
{
    public static PipelineProfile GetIisResetProfile()
    {
        var master = CreateProjectSettings(
            name: "Master",
            path: @"Server\Service.Master\Service.Master.csproj",
            msBuildEnable: false
        );

        var segment = CreateProjectSettings(
            name: "Segment",
            path: @"Server\Service.Segment\Service.Segment.csproj",
            msBuildEnable: false
        );

        return CreateProfile(
            name: "IIS Reset",
            projects: [master, segment],
            webEnable: true,
            httpMonitorEnable: true,
            masterLogDirectory: @"C:\HTTP\Raid\Master\log",
            segmentLogDirectory: @"C:\HTTP\Raid\Segment00\log"
        );
    }

    public static PipelineProfile GetStandardProfile()
    {
        var master = CreateProjectSettings(
            name: "Master",
            path: @"Server\Service.Master\Service.Master.csproj",
            msBuildEnable: true,
            msBuildParameters: ["/p:Configuration=Debug", "/p:PreBuildEvent=", "/t:build"],
            enableDeliveryBin: true
        );

        var segment = CreateProjectSettings(
            name: "Segment",
            path: @"Server\Service.Segment\Service.Segment.csproj",
            msBuildEnable: true,
            msBuildParameters: ["/p:Configuration=Debug", "/p:PreBuildEvent=", "/t:build"],
            enableDeliveryBin: true
        );

        return CreateProfile(
            name: "Standard Profile",
            projects: [master, segment],
            gitEnable: true,
            gitPull: true,
            sqlEnable: true,
            connectionString: "Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True",
            sqlScriptPath: @"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql",
            httpMonitorEnable: true,
            webEnable: true,
            webUrl: "http://localhost/Raid/Segment00/Segment.ashx",
            masterLogDirectory: @"C:\HTTP\Raid\Master\log",
            segmentLogDirectory: @"C:\HTTP\Raid\Segment00\log"
        );
    }

    public static PipelineProfile GetExtendedProfile(DirectoryModel solutionDirectory, DirectoryModel httpDirectory)
    {
        var master = CreateProjectSettings(
            name: "Master",
            path: @"Server\Service.Master\Service.Master.csproj",
            msBuildEnable: true,
            msBuildParameters: ["/p:Configuration=Debug", "/t:rebuild"],
            enableDeliveryBin: true
        );

        var segment = CreateProjectSettings(
            name: "Segment",
            path: @"Server\Service.Segment\Service.Segment.csproj",
            msBuildEnable: true,
            msBuildParameters: ["/p:Configuration=Debug", "/t:rebuild"],
            enableDeliveryBin: true
        );
        var dataBlender = CreateProjectSettings(
            name: "DataBlender",
            path: @"Utils\DataBlender\DataBlender.csproj",
            msBuildEnable: true,
            msBuildParameters: ["/p:Configuration=Debug", "/t:rebuild"],
            enableCustomDelivery: true,
            enableDeliveryBin: false,
            customDeliveryDirectories:
            [
                new DeliveryDirectory
                {
                    Source = Path.Combine(solutionDirectory.Path, @"Server\Service.Master\App_Data\Storage"),
                    Destination = Path.Combine(httpDirectory.Path, @"Master\App_Data\Storage\")
                }
            ],
            postBuildEvents:
            [
                new ProcessEvent
                {
                    Path = @"C:\Raid\Utils\DataBlender\bin\Debug\DataBlender.exe",
                    Arguments = ""
                }
            ]
        );

        return CreateProfile(
            name: "Extended Profile",
            projects: [master, segment, dataBlender],
            gitEnable: true,
            gitPull: true,
            sqlEnable: true,
            connectionString:
            "Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True",
            sqlScriptPath: @"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql",
            httpMonitorEnable: true,
            webEnable: true,
            webUrl: "http://localhost/Raid/Segment00/Segment.ashx",
            masterLogDirectory: @"C:\HTTP\Raid\Master\log",
            segmentLogDirectory: @"C:\HTTP\Raid\Segment00\log"
        );
    }

    private static ProjectSettings CreateProjectSettings(
        string name,
        string path,
        bool msBuildEnable,
        IEnumerable<string>? msBuildParameters = null,
        bool enableCustomDelivery = false,
        bool enableDeliveryBin = false,
        List<DeliveryDirectory>? customDeliveryDirectories = null,
        List<ProcessEvent>? postBuildEvents = null)
    {
        return new ProjectSettings
        {
            Project = new DirectoryModel { Name = name, Path = path },
            MsBuildSettings = new MsBuildSettings
            {
                Enable = msBuildEnable,
                Parameters = msBuildParameters?.ToList() ?? ["/p:Configuration=Debug", "/t:build"],
                PostBuildEvents = postBuildEvents ?? []
            },
            DeliverySettings = new DeliverySettings
            {
                EnableCustomDelivery = enableCustomDelivery,
                EnableDeliveryBin = enableDeliveryBin,
                CustomDeliveryDirectories = customDeliveryDirectories
            }
        };
    }

    private static PipelineProfile CreateProfile(
        string name,
        IEnumerable<ProjectSettings> projects,
        bool gitEnable = false,
        bool gitPull = false,
        bool sqlEnable = false,
        string? connectionString = null,
        string? sqlScriptPath = null,
        bool httpMonitorEnable = false,
        bool webEnable = true,
        string? webUrl = null,
        string? masterLogDirectory = null,
        string? segmentLogDirectory = null)
    {
        return new PipelineProfile
        {
            Name = name,
            SettingsPerProject = [.. projects],
            GitSettings = new GitSettings
            {
                Enable = gitEnable,
                ShouldPull = gitPull
            },
            SqlExecutionSettings = new SqlExecutionSettings
            {
                Enable = sqlEnable,
                ConnectionString = connectionString,
                PathToSqlScript = sqlScriptPath
            },
            WebBrowserSettings = new WebBrowserSettings
            {
                Enable = webEnable,
                Url = webUrl ?? "http://localhost/Raid/Segment00/Segment.ashx"
            },
            InternetInformationSettings = new InternetInformationSettings
            {
                Enable = true
            },
            MonitorLogFilesSettings = new LogMonitoringSettings
            {
                Enable = true,
                MasterLogDirectory = masterLogDirectory is null ? null : new DirectoryModel
                {
                    Name = $"{name} Master Log Directory",
                    Path = masterLogDirectory!,
                },
                SegmentLogDirectory = segmentLogDirectory is null ? null : new DirectoryModel
                {
                    Name = $"{name} Segment Log Directory",
                    Path = segmentLogDirectory!,
                },
            },
            HttpMonitoringSettings = new HttpMonitoringSettings
            {
                Enable = httpMonitorEnable,
                PingMaster = true,
                PingSegment = true,
                TimeoutMinutes = 5
            }
        };
    }
}