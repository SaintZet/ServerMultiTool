using ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Delivery;

public partial class DeliveryBinOperationWrapper(DeliveryBinOperation operation) : PipelineOperationWrapper(operation)
{
    public override string Name => "default name";
    public override string Description => "default description";
}
