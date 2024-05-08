using System.Diagnostics;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;

public class InternetInformationServices : ExecutionService
{
    public async Task<bool> StopAsync() => 
        await ExecuteCommandAsync("/stop");

    public async Task<bool> StartAsync() => 
        await ExecuteCommandAsync("/start");
    
    private async Task<bool> ExecuteCommandAsync(string arguments)
    {
        var info = new ProcessStartInfo("iisreset.exe", arguments);
        var response = await ProcessExecutor.StartProcessWithRetriesAsync(info, 10);
        
        return response.Success;
    }
}