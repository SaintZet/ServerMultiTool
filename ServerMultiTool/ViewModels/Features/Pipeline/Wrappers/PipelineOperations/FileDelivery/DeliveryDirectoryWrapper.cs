using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineOperations.FileDelivery;

public partial class DeliveryDirectoryWrapper : ObservableObject
{
    [ObservableProperty] private string _source;
    [ObservableProperty] private string _destination;

    public DeliveryDirectoryWrapper(DeliveryDirectory directory)
    {
        Source = directory.Source;
        Destination = directory.Destination;
    }

    public DeliveryDirectoryWrapper(DeliveryDirectoryWrapper directory)
    {
        Source = directory.Source;
        Destination = directory.Destination;
    }

    public DeliveryDirectory ToDeliveryDirectory() =>
        new() { Source = Source, Destination = Destination };

    internal DeliveryDirectoryWrapper Clone() =>
        new(this);
}
