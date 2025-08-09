using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using ServerMultiTool.ViewModels.Features.Settings.Wrappers;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Delivery;

public partial class DeliveryBinOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "Delivery Bin";

    [ObservableProperty] DirectoryModelWrapper _directory;

    public DeliveryBinOperationWrapper(DeliveryBinOperation operation) : base(operation)
    {
        if (operation.Project is not null)
            _directory = new DirectoryModelWrapper(operation.Project);
    }
}
