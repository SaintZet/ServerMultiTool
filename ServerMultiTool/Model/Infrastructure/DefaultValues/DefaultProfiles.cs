using System.IO;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.Model.Features.Pipeline.Operations.FileDelivery;
using ServerMultiTool.Model.Features.Pipeline.Operations.Network;
using ServerMultiTool.Model.Features.Pipeline.Profile;
using ServerMultiTool.Model.Features.Pipeline.Step;

namespace ServerMultiTool.Model.Infrastructure.DefaultValues;

public static class DefaultProfiles
{
    public static PipelineProfile GetIisResetProfile()
    {
        var pipelineProfile = new PipelineProfile("IIS Restart", "Profile for fast IIS reset with ping and open web page")
            .AddStep(new PipelineStep("IIS Stop", "This step resets the IIS server to apply changes.")
                .AddOperation(new ProcessExecutionOperation("IIS Stop")
                    .UpdateFileName("iisreset.exe")
                    .UpdateArguments("/stop")
                )
            )

            .AddStep(new PipelineStep("IIS Start", "This step starts the IIS server after reset.")
                .AddOperation(new ProcessExecutionOperation("IIS Start")
                    .UpdateFileName("iisreset.exe")
                    .UpdateArguments("/start")
                )
            )

            .AddStep(new PipelineStep("Http", "This step ping urls via http.")
                .AddOperation(new HttpPingOperation("Ping master")
                    .AddUrl("http://localhost/Raid/Master/Master.ashx")
                )
                .AddOperation(new HttpPingOperation("Ping segment")
                    .AddUrl("http://localhost/Raid/Segment00/Segment.ashx")
                )
            )

            .AddStep(new PipelineStep("Browser", "This step opens the web browser to the specified URL.")
                //.AddOperation(new WebBrowserOperation("Open Segment", "http://localhost/Raid/Segment00/Segment.ashx"))
                .AddOperation(new WebBrowserOperation("Open GBO Console")
                    .AddUrl("https://raid-gbo.x-plarium.com/#/console?serverId=746"))
                )
            ;

        return pipelineProfile;
    }

    public static PipelineProfile GetStandardProfile()
    {
        var pipelineProfile = new PipelineProfile("Standart Profile", "Standard deployment profile for regular working days")

            .AddStep(new PipelineStep("Git", "This step execute operations releated to Git.")
                .AddOperation(new GitPullOperation("Git Pull"))
            )

            .AddStep(new PipelineStep("MsBuild", "This step builds the projects using MSBuild.")

                .AddOperation(new MsBuildOperation("Build Master")
                    .UpdateProject(GetMasterProjectDirectory())
                    .AddParameter("/t:build").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))

                .AddOperation(new MsBuildOperation("Build Segment")
                    .UpdateProject(GetSegmentProjectDirectory())
                    .AddParameter("/t:build").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))
            )

            .AddStep(new PipelineStep("IIS Stop", "This step resets the IIS server to apply changes.")
                .AddOperation(new ProcessExecutionOperation("IIS Stop")
                    .UpdateFileName("iisreset.exe")
                    .UpdateArguments("/stop")
                )
            )

            .AddStep(new PipelineStep("Delivery", "This step delivers the built projects to the specified directories.")
                .AddOperation(new DeliveryBinOperation("Delivery Master")
                    .UpdateProject(GetMasterProjectDirectory())
                )
                .AddOperation(new DeliveryBinOperation("Delivery Segment")
                    .UpdateProject(GetSegmentProjectDirectory())
                )
            )

            .AddStep(new PipelineStep("Sql", "This step start Sql operations.")
                .AddOperation(new SqlExecutionOperation("Execute SQL Script")
                    .UpdateConnectionString("Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True")
                    .UpdatePathToSqlScript(@"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql"))
            )

            .AddStep(new PipelineStep("IIS Start", "This step starts the IIS server after reset.")
                .AddOperation(new ProcessExecutionOperation("IIS Start")
                    .UpdateFileName("iisreset.exe")
                    .UpdateArguments("/start")
                )
            )

            .AddStep(new PipelineStep("Http", "This step ping urls via http.")
                .AddOperation(new HttpPingOperation("Ping master")
                    .AddUrl("http://localhost/Raid/Master/Master.ashx")
                )
                .AddOperation(new HttpPingOperation("Ping segment")
                    .AddUrl("http://localhost/Raid/Segment00/Segment.ashx")
                )
            )

            .AddStep(new PipelineStep("Browser", "This step opens the web browser to the specified URL.")
                //.AddOperation(new WebBrowserOperation("Open Segment", "http://localhost/Raid/Segment00/Segment.ashx"))
                .AddOperation(new WebBrowserOperation("Open GBO Console")
                    .AddUrl("https://raid-gbo.x-plarium.com/#/console?serverId=746")
                )
            )
            ;

        return pipelineProfile;
    }

    public static PipelineProfile GetExtendedProfile(DirectoryModel solutionDirectory, DirectoryModel httpDirectory)
    {
        return new PipelineProfile("Extended Profile", "Advanced deployment profile with additional StaticData processing using DataBlender")

            .AddStep(new PipelineStep("Git", "This step execute operations releated to Git.")
                .AddOperation(new GitPullOperation("Git Pull"))
            )

            .AddStep(new PipelineStep("MsBuild", "This step builds the projects using MSBuild.")
                .AddOperation(new MsBuildOperation("Build Master")
                    .UpdateProject(GetMasterProjectDirectory())
                    .AddParameter("/t:rebuild").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))

                .AddOperation(new MsBuildOperation("Build Segment")
                    .UpdateProject(GetSegmentProjectDirectory())
                    .AddParameter("/t:rebuild").AddParameter("/p:Configuration=Debug").AddParameter("/p:PreBuildEvent="))
            )

            .AddStep(new PipelineStep("StaticData", "This step runs the Data Blender tool to process data.")

                .AddOperation(new MsBuildOperation("Build DataBlender")
                    .UpdateProject(GetDataBlenderProjectDirectory())
                    .AddParameter("/t:build").AddParameter("/p:Configuration=Debug")
                )

                .AddOperation(new ProcessExecutionOperation("Execute DataBlender")
                    .UpdateFileName(Path.Combine(solutionDirectory.Path, @"Utils\DataBlender\bin\Debug\DataBlender.exe"))
                    .UpdateArguments(string.Empty)
                    .UpdateRetryCount(0)
                )

                .AddOperation(new DeliverySpecifiedFilesOperation("Delivery StaticData")
                    .AddDeliveryDirectories(
                        source: Path.Combine(solutionDirectory.Path, @"Server\Service.Master\App_Data\Storage"),
                        destination: Path.Combine(httpDirectory.Path, @"Master\App_Data\Storage\")
                    )
                )
            )

            .AddStep(new PipelineStep("IIS Stop", "This step resets the IIS server to apply changes.")
                .AddOperation(new ProcessExecutionOperation("IIS Stop")
                    .UpdateFileName("iisreset.exe")
                    .UpdateArguments("/stop")
                )
            )

            .AddStep(new PipelineStep("Delivery", "This step delivers the built projects to the specified directories.")
                .AddOperation(new DeliveryBinOperation("Delivery Master")
                    .UpdateProject(GetMasterProjectDirectory())
                )
                .AddOperation(new DeliveryBinOperation("Delivery Segment")
                    .UpdateProject(GetSegmentProjectDirectory())
                )
            )

            .AddStep(new PipelineStep("Sql", "This step start Sql operations.")
                .AddOperation(new SqlExecutionOperation("Execute SQL Script")
                    .UpdateConnectionString("Server=PLSCHEPETS;Database=raidMaster;User Id=geotopia;Password=super;TrustServerCertificate=True")
                    .UpdatePathToSqlScript(@"C:\ServerDeployTool\RaidDeploy\BatchFiles\script.sql"))
            )

            .AddStep(new PipelineStep("IIS Start", "This step starts the IIS server after reset.")
                .AddOperation(new ProcessExecutionOperation("IIS Start")
                    .UpdateFileName("iisreset.exe")
                    .UpdateArguments("/start")
                )
            )

            .AddStep(new PipelineStep("Http", "This step ping urls via http.")
                .AddOperation(new HttpPingOperation("Ping master")
                    .AddUrl("http://localhost/Raid/Master/Master.ashx")
                )
                .AddOperation(new HttpPingOperation("Ping segment")
                    .AddUrl("http://localhost/Raid/Segment00/Segment.ashx")
                )
            )

            .AddStep(new PipelineStep("Browser", "This step opens the web browser to the specified URL.")
                //.AddOperation(new WebBrowserOperation("Open Segment", "http://localhost/Raid/Segment00/Segment.ashx"))
                .AddOperation(new WebBrowserOperation("Open GBO Console")
                    .AddUrl("https://raid-gbo.x-plarium.com/#/console?serverId=746"))
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

}
