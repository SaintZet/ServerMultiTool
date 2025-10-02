using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.FileDelivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Delivery;

public partial class DeliverySpecifiedFilesOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "Delivery Specified Files";

    [ObservableProperty] ObservableCollection<DeliveryDirectoryWrapper> _directories = [];

    public DeliverySpecifiedFilesOperationWrapper(DeliverySpecifiedFilesOperation operation) : base(operation)
    {
        foreach (var directory in operation.CustomDeliveryDirectories)
        {
            Directories.Add(new DeliveryDirectoryWrapper(directory));
        }

        Directories.CollectionChanged += Directories_CollectionChanged;
    }

    private void Directories_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var op = (DeliverySpecifiedFilesOperation)Operation;
        op.CustomDeliveryDirectories = Directories
            .Select(d => d.ToDeliveryDirectory())
            .ToList();
    }

    [RelayCommand]
    private void AddDeliveryDirectory(DeliveryDirectoryWrapper? directory)
    {
        if (directory is null)
            return;

        if (string.IsNullOrWhiteSpace(directory.Source) || string.IsNullOrWhiteSpace(directory.Destination))
            return;

        // prevent duplicates by Source+Destination
        var exists = Directories.Any(d =>
            string.Equals(d.Source, directory.Source, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(d.Destination, directory.Destination, StringComparison.OrdinalIgnoreCase));

        if (!exists)
        {
            Directories.Add(new DeliveryDirectoryWrapper(directory));
        }
    }

    [RelayCommand]
    private void RemoveDirectory(DeliveryDirectoryWrapper? directory)
    {
        if (directory is null)
            return;

        var existing = Directories.FirstOrDefault(d => ReferenceEquals(d, directory) ||
            (string.Equals(d.Source, directory.Source, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(d.Destination, directory.Destination, StringComparison.OrdinalIgnoreCase)));

        if (existing is not null)
            Directories.Remove(existing);
    }

    public override PipelineOperationBase ToOriginal()
    {
        base.ToOriginal();

        var op = (DeliverySpecifiedFilesOperation)Operation;
        op.CustomDeliveryDirectories = Directories
            .Select(d => d.ToDeliveryDirectory())
            .ToList();

        return op;
    }
}
