using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class InternetInformationServices(string arguments) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var info = new ProcessStartInfo("iisreset.exe", arguments);
        
        //todo: add retryCount parameter to settings
        var response = await ProcessExecutor.StartProcessWithRetriesAsync(info, 2, cancellationToken);
        
        return response.Success ? OperationResult.Success : OperationResult.Failure;
    }
}