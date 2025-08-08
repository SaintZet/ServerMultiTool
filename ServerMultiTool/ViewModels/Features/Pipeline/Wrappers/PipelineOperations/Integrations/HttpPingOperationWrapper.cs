using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class HttpPingOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    public string Name => "HTTP Ping";
    public string Description => "Pings a specified HTTP endpoint to check its availability.";

    readonly HttpPingOperation _operation;

    public HttpPingOperationWrapper(HttpPingOperation operation)
    {
        _operation = operation;

        Enabled = operation.Enabled;
    }

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _operation.ExecuteAsync(cancellationToken);
    }

    public PipelineOperation ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        return _operation;
    }

    public void UpdateSolutionDirectory(DirectoryModel directory)
    {
        _operation.UpdateSolutionDirectory(directory);
    }

    public void UpdateHttpDirectory(DirectoryModel directory)
    {
        _operation.UpdateHttpDirectory(directory);
    }
}