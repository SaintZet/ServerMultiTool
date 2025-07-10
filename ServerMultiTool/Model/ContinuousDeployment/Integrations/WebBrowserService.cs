using System.Diagnostics;
using System.Threading.Tasks;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class WebBrowserService(WebBrowserSettings settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        if (string.IsNullOrEmpty(settings.Url))
            return OperationResult.Cancelled;
        
        await OpenPageAsync(settings.Url);
        
        Logger.LogInfoWithPublish("The web page has been successfully opened.");
        
        return OperationResult.Success;
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