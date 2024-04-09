using System.Threading.Tasks;
using ServerMultiTool.Models.Build.Contracts;
using ServerMultiTool.Models.Deployment.Contracts;
using ServerMultiTool.Models.GeneralPipeline.Contracts;
using ServerMultiTool.Models.Integrations.Git.Contracts;
using ServerMultiTool.Models.Integrations.Sql.Contracts;
using ServerMultiTool.Models.Integrations.WebBrowser.Contracts;
using ServerMultiTool.Models.Settings.Profiles.Data;

namespace ServerMultiTool.Models.GeneralPipeline.Services;

public class GeneralPipelineService : IGeneralPipelineService
{
    private readonly IBuildService _buildService;
    private readonly IDeployService _deployService;
    private readonly IInternetInformationServices _internetInformationServices;
    private readonly IGitIntegrationService _gitIntegrationService;
    private readonly ISqlExecutionService _sqlExecutionService;
    private readonly IWebBrowserService _webBrowserService;

    public GeneralPipelineService(
        IGitIntegrationService gitIntegrationService,
        IBuildService buildService,
        IDeployService deployService,
        IInternetInformationServices internetInformationServices,
        ISqlExecutionService sqlExecutionService,
        IWebBrowserService webBrowserService)
    {
        _gitIntegrationService = gitIntegrationService;
        _buildService = buildService;
        _deployService = deployService;
        _internetInformationServices = internetInformationServices;
        _sqlExecutionService = sqlExecutionService;
        _webBrowserService = webBrowserService;
    }

    public async Task ExecuteGeneralPipeline(SettingsProfile settings)
    {
        var gitSettings = settings.GitIntegrationSettings;
        await _gitIntegrationService.ExecuteGitOperationsAsync(gitSettings);
            
        var buildSettings = settings.BuildSettings;
        await _buildService.ExecuteBuildAsync(buildSettings);

        var deploySettings = settings.DeploySettings;
        await _deployService.ExecuteDeployAsync(buildSettings, deploySettings);

        await _internetInformationServices.StopAsync();
        
        var sqlSettings = settings.SqlIntegrationSettings;
        await _sqlExecutionService.ExecuteSqlScriptAsync(sqlSettings);

        await _internetInformationServices.StartAsync();
        
        var webBrowserSettings = settings.WebBrowserSettings;
        await _webBrowserService.OpenUrlInDefaultBrowserAsync(webBrowserSettings);
    }
}