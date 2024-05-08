using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;

public class WebBrowserService : ExecutionService
{
    public async Task<bool> ExecuteAsync(PipelineProfile pipeline)
    {
        var settings = pipeline.WebBrowserSettings;
        if (settings.Enable is false)
        {
            Logger.LogInfo($"Opening URLs is disabled by {nameof(PipelineProfile)}.");
            
            return true;
        }
    
        try
        {
            await OpenPageAsync(settings.Url);
            Logger.LogInfo("The web page has been successfully opened.");
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to open the URL: \n{ex.Message}");
            
            return false;
        }
    }

    private static async Task OpenPageAsync(string url)
    {
        await Task.Run(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        });
    }
}