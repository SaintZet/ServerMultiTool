using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ServerMultiTool.Model.Common;
using ServerMultiTool.ViewModels.Features.Settings.Wrappers;

namespace ServerMultiTool.ViewModels.Features.Settings.Extensions;

public static class DirectoryModelExtensions
{
    public static ObservableCollection<DirectoryModelWrapper> ToWrapperCollection(this IEnumerable<DirectoryModel> models) =>
        new(models.Select(model => new DirectoryModelWrapper(model)));

    public static DirectoryModel[] ToStructArray(this IEnumerable<DirectoryModelWrapper> models) =>
        models.Select(wrapper =>
            new DirectoryModel
            {
                Name = wrapper.Name,
                Path = wrapper.Path
            }).ToArray();

    public static ObservableCollection<DirectoryModelWrapper> Clone(this IEnumerable<DirectoryModelWrapper> wrappers)
    {
        var clonedCollection = new ObservableCollection<DirectoryModelWrapper>();

        foreach (var wrapper in wrappers)
            clonedCollection.Add(new DirectoryModelWrapper(wrapper.Name, wrapper.Path));

        return clonedCollection;
    }

    public static ObservableCollection<DirectoryModelWrapper> CloneWithPropertyChanged(this IEnumerable<DirectoryModelWrapper> wrappers, PropertyChangedEventHandler handler)
    {
        var clonedCollection = new ObservableCollection<DirectoryModelWrapper>();

        foreach (var wrapper in wrappers)
        {
            var clonedWrapper = new DirectoryModelWrapper(wrapper.Name, wrapper.Path);
            clonedWrapper.PropertyChanged += handler;
            clonedCollection.Add(clonedWrapper);
        }

        return clonedCollection;
    }
}
