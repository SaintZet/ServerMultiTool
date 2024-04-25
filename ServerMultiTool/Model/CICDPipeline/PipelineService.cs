using System;
using System.Threading.Tasks;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline;

public static class PipelineService
{
    public static async Task ExecutePipeline(PipelineProfile pipeline)
    {
        try
        {
            await GitService.ExecuteAsync(pipeline);
            await MsBuildService.ExecuteAsync(pipeline);
            await DeliveryService.ExecuteAsync(pipeline);
            await InternetInformationServices.StopAsync();
            await SqlExecutionService.ExecuteAsync(pipeline);
            await InternetInformationServices.StartAsync();
            await WebBrowserService.ExecuteAsync(pipeline);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}