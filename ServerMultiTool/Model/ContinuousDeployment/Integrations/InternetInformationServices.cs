using System.Diagnostics;
using System.Threading.Tasks;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class InternetInformationServices : PipelineOperation
{
    private readonly string _arguments;

    public InternetInformationServices(string arguments) => 
        _arguments = arguments;

    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        var info = new ProcessStartInfo("iisreset.exe", _arguments);
        var response = await ProcessExecutor.StartProcessWithRetriesAsync(info, 2);
        
        return response.Success ? OperationResult.Success : OperationResult.Failure;
    }
}