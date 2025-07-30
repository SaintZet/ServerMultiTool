using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using System.Linq;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Managers;

public class PipelineOperationFactory
{
    private const string IIS_STOP_COMMAND = "/stop";
    private const string IIS_START_COMMAND = "/start";

    public static PipelineOperationCollection CreatePipelineOperations(PipelineProfile pipeline)
    {
        var operations = new PipelineOperationCollection();

        if (pipeline.GitSettings.Enable)
            operations.Add(new(new GitService(pipeline.GitSettings), "Git"));

        if (pipeline.SettingsPerProject.Any(x => x.MsBuildSettings.Enable))
            operations.Add(new(new MsBuildService(pipeline.SettingsPerProject), "MsBuild"));

        if (pipeline.InternetInformationSettings.Enable)
            operations.Add(new(new InternetInformationServices(IIS_STOP_COMMAND, pipeline.InternetInformationSettings), "IIS Stop"));

        if (pipeline.SettingsPerProject.Any(x => x.DeliverySettings.EnableCustomDelivery || x.DeliverySettings.EnableDeliveryBin))
            operations.Add(new(new DeliveryService(pipeline.SettingsPerProject), "Delivery"));

        if (pipeline.SqlExecutionSettings.Enable)
            operations.Add(new(new SqlExecutionService(pipeline.SqlExecutionSettings), "SQL"));

        if (pipeline.InternetInformationSettings.Enable)
            operations.Add(new(new InternetInformationServices(IIS_START_COMMAND, pipeline.InternetInformationSettings), "IIS Start"));

        if (pipeline.WebBrowserSettings.Enable)
            operations.Add(new(new WebBrowserService(pipeline.WebBrowserSettings), "Web"));

        if (pipeline.HttpMonitoringSettings.Enable)
            operations.Add(new(new HttpMonitoringService(pipeline.HttpMonitoringSettings), "Http"));

        return operations;
    }
}