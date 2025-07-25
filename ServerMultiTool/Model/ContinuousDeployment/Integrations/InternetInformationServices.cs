using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class InternetInformationServices(string arguments) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        try
        {
            var info = new ProcessStartInfo("iisreset.exe", arguments);

            //todo: add retryCount parameter to settings
            var response = await ProcessExecutor.StartProcessWithRetriesAsync(info, 2, cancellationToken);

            return response.Success ? OperationResult.Success : OperationResult.Failure;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled;
        }
    }
}