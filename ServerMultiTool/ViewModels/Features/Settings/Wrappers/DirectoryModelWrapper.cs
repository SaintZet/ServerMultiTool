using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;

namespace ServerMultiTool.ViewModels.Features.Settings.Wrappers;

public class DirectoryModelWrapper : ObservableObject
{
    private readonly DirectoryModel _directoryModel;

    public DirectoryModelWrapper(DirectoryModel directoryModel) =>
        _directoryModel = directoryModel;

    public DirectoryModelWrapper(string name, string path) =>
        _directoryModel = new DirectoryModel { Name = name, Path = path, };

    public string Name
    {
        get => _directoryModel.Name;
        set
        {
            if (_directoryModel.Name == value)
                return;

            _directoryModel.Name = value;
            OnPropertyChanged();
        }
    }

    public string Path
    {
        get => _directoryModel.Path;
        set
        {
            if (_directoryModel.Path == value)
                return;

            _directoryModel.Path = value;
            OnPropertyChanged();
        }
    }

    public DirectoryModel ToOriginal()
    {
        return _directoryModel;
    }
}
