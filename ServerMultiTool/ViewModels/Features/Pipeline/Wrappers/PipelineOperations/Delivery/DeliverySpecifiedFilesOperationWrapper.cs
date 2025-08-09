using ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Delivery;

public partial class DeliverySpecifiedFilesOperationWrapper(DeliverySpecifiedFilesOperation operation) : PipelineOperationWrapper(operation)
{
    public override string Name { get; set; } = "default name";
    public override string Description { get; set; } = "default description";
}
