using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common.Logs;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Management;

public partial class PipelineLogTabViewModel : ObservableObject
{
    [ObservableProperty] private string _header = string.Empty;
    [ObservableProperty] private string _path = string.Empty;

    public ObservableCollection<LogEvent> Messages { get; } = [];

    public ICollectionView LogView { get; }

    public PipelineLogTabViewModel(string header, string path = "")
    {
        Header = header;
        Path = path;
        LogView = CollectionViewSource.GetDefaultView(Messages);
    }
}


