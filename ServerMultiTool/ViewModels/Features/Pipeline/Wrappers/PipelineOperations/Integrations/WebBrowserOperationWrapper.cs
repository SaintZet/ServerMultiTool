using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class WebBrowserOperationWrapper(WebBrowserOperation operation) : PipelineOperationWrapper(operation)
{
    public override string Name => "Web Browser Operation";
    public override string Description => "This operation opens a web browser to a specified URL.";
}