using ServerMultiTool.Model.Pipeline.Contracts;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class WebBrowserService(WebBrowserSettings settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(settings.Url))
        {
            Logger.LogErrorWithPublish("The URL is not specified in the settings.");
            return OperationResult.Failure;
        }

        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        await OpenPageAsync(settings.Url, cancellationToken);

        Logger.LogInfoWithPublish("The web page has been successfully opened.");

        return OperationResult.Success;
    }

    private static async Task OpenPageAsync(string url, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }, cancellationToken);
    }
}