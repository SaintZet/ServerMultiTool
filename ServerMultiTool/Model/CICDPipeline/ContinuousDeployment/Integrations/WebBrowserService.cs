using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;

public static class WebBrowserService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(WebBrowserService));
    
    public static async Task<bool> ExecuteAsync(PipelineProfile pipeline)
    {
        var settings = pipeline.WebBrowserSettings;
        if (settings.Enable is false)
        {
            Log.Info($"Opening URLs is disabled by {nameof(PipelineProfile)}.");
            
            return true;
        }
    
        try
        {
            await OpenPageAsync(settings.Url);
            Log.Info("The web page has been successfully opened.");
            
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open the URL: \n{ex.Message}");
            
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