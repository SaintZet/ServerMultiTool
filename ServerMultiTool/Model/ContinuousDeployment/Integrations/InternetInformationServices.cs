using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class InternetInformationServices(string arguments, InternetInformationSettings settings) : PipelineOperation
{
    private readonly string _arguments = arguments;
    private readonly InternetInformationSettings _settings = settings ?? new InternetInformationSettings();

    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        try
        {
            var info = new ProcessStartInfo("iisreset.exe", _arguments);
            var response = await ProcessExecutor.StartProcessWithRetriesAsync(info, _settings.RetryCount, cancellationToken);

            return response.Success ? OperationResult.Success : OperationResult.Failure;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled;
        }
    }
}