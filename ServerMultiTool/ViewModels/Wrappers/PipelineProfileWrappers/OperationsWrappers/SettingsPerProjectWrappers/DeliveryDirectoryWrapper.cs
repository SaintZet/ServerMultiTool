using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers.SettingsPerProjectWrappers;

public partial class DeliveryDirectoryWrapper : BaseObservableWrapper
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