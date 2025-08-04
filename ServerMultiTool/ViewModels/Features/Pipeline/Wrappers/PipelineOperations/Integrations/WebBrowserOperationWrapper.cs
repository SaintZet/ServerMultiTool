using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Domain.Pipeline.Interfaces;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class WebBrowserOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    public string Name => "Web Browser Operation";
    public string Description => "This operation opens a web browser to a specified URL.";

    readonly WebBrowserOperation _operation;

    public WebBrowserOperationWrapper(WebBrowserOperation operation)
    {
        _operation = operation;

        Enabled = operation.Enabled;
    }

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _operation.ExecuteAsync(cancellationToken);
    }

    public IPipelineOperation ToOriginal()
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