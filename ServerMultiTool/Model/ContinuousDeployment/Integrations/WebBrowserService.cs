using System.Diagnostics;
using System.Threading.Tasks;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class WebBrowserService : PipelineOperation
{
    private readonly WebBrowserSettings _settings;

    public WebBrowserService(WebBrowserSettings settings) => 
        _settings = settings;

    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        await OpenPageAsync(_settings.Url);
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