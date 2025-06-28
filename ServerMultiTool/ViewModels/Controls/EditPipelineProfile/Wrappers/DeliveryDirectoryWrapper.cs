using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

public partial class DeliveryDirectoryWrapper : ObservableObject
{
    [ObservableProperty]
    private string _source;

    [ObservableProperty]
    private string _destination;

    public DeliveryDirectoryWrapper(DeliveryDirectories directory)
    {
        Source = directory.Source;
        Destination = directory.Destination;
    }

    public DeliveryDirectories ToDeliveryDirectory() => new()
    {
        Source = Source,
        Destination = Destination
    };
}