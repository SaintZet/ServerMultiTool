using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline;
using System.IO;

namespace ServerMultiTool.Model.Common.DefaultValues;

public static class DefaultProfiles
{
    public static PipelineProfile GetIisResetProfile()
    {
        var pipelineProfile = new PipelineProfile("IIS Restart", "Profile for fast IIS reset with ping and open web page")
            .UpdateGsLogMonitoringSettings(GetDefaultGsLogMonitoringSettings())
            .AddStep(new PipelineStep("IIS Stop", "This step resets the IIS server to apply changes.")
                .AddOperation(new ProcessExecutionOperation("IIS Stop", "iisreset.exe", "/stop"))
                )

            .AddStep(new PipelineStep("IIS Start", "This step starts the IIS server after reset.")
                .AddOperation(new ProcessExecutionOperation("IIS Start", "iisreset.exe", "/start"))
                )

            .AddStep(new PipelineStep("Http", "This step ping urls via http.")
                .AddOperation(new HttpPingOperation("Ping master", "http://localhost/Raid/Master/Master.ashx"))
                .AddOperation(new HttpPingOperation("Ping segment", "http://localhost/Raid/Segment00/Segment.ashx"))
                )

            .AddStep(new PipelineStep("Web Browser", "This step opens the web browser to the specified URL.")
                //.AddOperation(new WebBrowserOperation("Open Segment", "http://localhost/Raid/Segment00/Segment.ashx"))
                .AddOperation(new WebBrowserOperation("Open GBO Console", "https://raid-gbo.x-plarium.com/#/console?serverId=746"))
                )
            ;

        return pipelineProfile;
    }

    public static PipelineProfile GetStandardProfile()
    {
        var pipelineProfile = new PipelineProfile("Standart Profile", "")
            .UpdateGsLogMonitoringSettings(GetDefaultGsLogMonitoringSettings())

            .AddStep(new PipelineStep("Git", "This step execute operations releated to Git.")
                .AddOperation(new GitPullOperation("Git Pull"))
            )

            .AddStep(new PipelineStep("MsBuild", "This step builds the projects using MSBuild.")

                .AddOperation(new MsBuildOperation("Build Master", GetMasterProjectDirectory())
                    .AddParameter("/t:build").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))

                .AddOperation(new MsBuildOperation("Build Segment", GetSegmentProjectDirectory())
                    .AddParameter("/t:build").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))
            )

            .AddStep(new PipelineStep("IIS Stop", "This step resets the IIS server to apply changes.")
                .AddOperation(new ProcessExecutionOperation("IIS Stop", "iisreset.exe", "/stop"))
            )

            .AddStep(new PipelineStep("Delivery", "This step delivers the built projects to the specified directories.")
                .AddOperation(new DeliveryBinOperation("Delivery Master", GetMasterProjectDirectory()))
                .AddOperation(new DeliveryBinOperation("Delivery Segment", GetSegmentProjectDirectory()))
            )

            .AddStep(new PipelineStep("Sql", "This step start Sql operations.")
                .AddOperation(new SqlExecutionOperation("Execute SQL Script")
                    .UpdateConnectionString("Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True")
                    .UpdatePathToSqlScript(@"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql"))
            )

            .AddStep(new PipelineStep("IIS Start", "This step starts the IIS server after reset.")
                .AddOperation(new ProcessExecutionOperation("IIS Start", "iisreset.exe", "/start"))
            )

            .AddStep(new PipelineStep("Http", "This step ping urls via http.")
                .AddOperation(new HttpPingOperation("Ping master", "http://localhost/Raid/Master/Master.ashx"))
                .AddOperation(new HttpPingOperation("Ping segment", "http://localhost/Raid/Segment00/Segment.ashx"))
            )

            .AddStep(new PipelineStep("Web Browser", "This step opens the web browser to the specified URL.")
                //.AddOperation(new WebBrowserOperation("Open Segment", "http://localhost/Raid/Segment00/Segment.ashx"))
                .AddOperation(new WebBrowserOperation("Open GBO Console", "https://raid-gbo.x-plarium.com/#/console?serverId=746"))
            )
            ;

        return pipelineProfile;
    }

    public static PipelineProfile GetExtendedProfile(DirectoryModel solutionDirectory, DirectoryModel httpDirectory)
    {
        return new PipelineProfile("Extended Profile", "")
            .UpdateGsLogMonitoringSettings(GetDefaultGsLogMonitoringSettings())

            .AddStep(new PipelineStep("Git", "This step execute operations releated to Git.")
                .AddOperation(new GitPullOperation("Git Pull"))
            )

            .AddStep(new PipelineStep("MsBuild", "This step builds the projects using MSBuild.")
                .AddOperation(new MsBuildOperation("Build Master", GetMasterProjectDirectory())
                    .AddParameter("/t:rebuild").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))

                .AddOperation(new MsBuildOperation("Build Segment", GetSegmentProjectDirectory())
                    .AddParameter("/t:rebuild").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))
            )

            .AddStep(new PipelineStep("StaticData", "This step runs the Data Blender tool to process data.")

                .AddOperation(new MsBuildOperation("Build DataBlender", GetDataBlenderProjectDirectory())
                    .AddParameter("/t:build").AddParameter("/p:Configuration=Debug")
                )

                .AddOperation(new ProcessExecutionOperation("Execute DataBlender", fileName: Path.Combine(solutionDirectory.Path, @"Utils\DataBlender\bin\Debug\DataBlender.exe"))
                    .UpdateArguments(string.Empty).UpdateRetryCount(0)
                )

                .AddOperation(new DeliverySpecifiedFilesOperation("Delivery StaticData")
                    .AddDeliveryDirectories(
                        source: Path.Combine(solutionDirectory.Path, @"Server\Service.Master\App_Data\Storage"),
                        destination: Path.Combine(httpDirectory.Path, @"Master\App_Data\Storage\")
                    )
                )
            )

            .AddStep(new PipelineStep("IIS Stop", "This step resets the IIS server to apply changes.")
                .AddOperation(new ProcessExecutionOperation("IIS Stop", "iisreset.exe", "/stop"))
            )

            .AddStep(new PipelineStep("Delivery", "This step delivers the built projects to the specified directories.")
                .AddOperation(new DeliveryBinOperation("Delivery Master", GetMasterProjectDirectory()))
                .AddOperation(new DeliveryBinOperation("Delivery Segment", GetSegmentProjectDirectory()))
            )

            .AddStep(new PipelineStep("Sql", "This step start Sql operations.")
                .AddOperation(new SqlExecutionOperation("Execute SQL Script")
                    .UpdateConnectionString("Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True")
                    .UpdatePathToSqlScript(@"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql"))
            )

            .AddStep(new PipelineStep("IIS Start", "This step starts the IIS server after reset.")
                .AddOperation(new ProcessExecutionOperation("IIS Start", "iisreset.exe", "/start"))
            )

            .AddStep(new PipelineStep("Http", "This step ping urls via http.")
                .AddOperation(new HttpPingOperation("Ping master", "http://localhost/Raid/Master/Master.ashx"))
                .AddOperation(new HttpPingOperation("Ping segment", "http://localhost/Raid/Segment00/Segment.ashx"))
            )

            .AddStep(new PipelineStep("Web Browser", "This step opens the web browser to the specified URL.")
                //.AddOperation(new WebBrowserOperation("Open Segment", "http://localhost/Raid/Segment00/Segment.ashx"))
                .AddOperation(new WebBrowserOperation("Open GBO Console", "https://raid-gbo.x-plarium.com/#/console?serverId=746"))
            )
            ;
    }

    private static DirectoryModel GetMasterProjectDirectory()
    {
        return new DirectoryModel
        {
            Name = "Master",
            Path = @"Server\Service.Master\Service.Master.csproj"
        };
    }

    private static DirectoryModel GetSegmentProjectDirectory()
    {
        return new DirectoryModel
        {
            Name = "Segment",
            Path = @"Server\Service.Segment\Service.Segment.csproj"
        };
    }

    private static DirectoryModel GetDataBlenderProjectDirectory()
    {
        return new DirectoryModel
        {
            Name = "DataBlender Project",
            Path = @"Utils\DataBlender\DataBlender.csproj"
        };
    }

    private static GsLogMonitoringSettings GetDefaultGsLogMonitoringSettings()
    {
        return new GsLogMonitoringSettings
        {
            Enable = true,
            MasterLogDirectory = new DirectoryModel
            {
                Name = "Master",
                Path = @"C:\HTTP\Raid\Master\log"
            },
            SegmentLogDirectory = new DirectoryModel
            {
                Name = "Segment",
                Path = @"C:\HTTP\Raid\Segment00\log"
            }
        };
    }
}