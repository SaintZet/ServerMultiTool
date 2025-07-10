using System.Diagnostics;
using System.Threading.Tasks;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class InternetInformationServices(string arguments) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        var info = new ProcessStartInfo("iisreset.exe", arguments);
        var response = await ProcessExecutor.StartProcessWithRetriesAsync(info, 2);
        
        return response.Success ? OperationResult.Success : OperationResult.Failure;
    }
}