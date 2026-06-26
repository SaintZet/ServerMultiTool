using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.FileDelivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using ServerMultiTool.ViewModels.Features.Settings.Wrappers;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineOperations.FileDelivery;

public partial class DeliveryBinOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "Delivery Bin";

    [ObservableProperty] DirectoryModelWrapper? _directory;

    public DeliveryBinOperationWrapper(DeliveryBinOperation operation) : base(operation)
    {
        if (operation.Project is not null)
            Directory = new DirectoryModelWrapper(operation.Project);
    }

    public override PipelineOperationBase ToOriginal()
    {
        base.ToOriginal();

        var op = (DeliveryBinOperation)Operation;
        if (Directory is not null)
            op.UpdateProject(Directory.ToOriginal());

        return op;
    }
}
