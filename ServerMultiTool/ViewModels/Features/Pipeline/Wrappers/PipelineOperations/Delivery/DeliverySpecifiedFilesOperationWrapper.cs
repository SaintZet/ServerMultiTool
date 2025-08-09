using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Collections.ObjectModel;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Delivery;

public partial class DeliverySpecifiedFilesOperationWrapper : PipelineOperationWrapper
{
    public override string Name { get; set; } = "default name";
    public override string Description { get; set; } = "default description";

    [ObservableProperty] ObservableCollection<DeliveryDirectoryWrapper> _directories = [];

    public DeliverySpecifiedFilesOperationWrapper(DeliverySpecifiedFilesOperation operation) : base(operation)
    {
        foreach (var directory in operation.CustomDeliveryDirectories)
        {
            Directories.Add(new DeliveryDirectoryWrapper(directory));
        }
    }
}
