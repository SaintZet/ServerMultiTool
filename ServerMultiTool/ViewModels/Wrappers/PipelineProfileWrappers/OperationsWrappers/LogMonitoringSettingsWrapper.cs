using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;
using ServerMultiTool.ViewModels.Pages.Settings.Wrappers;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class LogMonitoringSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty]
    private bool _enable;

    [ObservableProperty]
    private DirectoryModelWrapper _masterLogDirectory;

    [ObservableProperty]
    private DirectoryModelWrapper _segmentLogDirectory;

    public LogMonitoringSettingsWrapper(LogMonitoringSettings settings)
    {
        Enable = settings.Enable;

        if (settings.MasterLogDirectory != null)
        {
            MasterLogDirectory = new DirectoryModelWrapper(
                settings.MasterLogDirectory.Name ?? string.Empty,
                settings.MasterLogDirectory.Path ?? string.Empty);
        }
        else
        {
            MasterLogDirectory = new DirectoryModelWrapper("Master Logs", string.Empty);
        }

        MasterLogDirectory.PropertyChanged += (_, _) => OnPropertyChanged(string.Empty);

        if (settings.SegmentLogDirectory != null)
        {
            SegmentLogDirectory = new DirectoryModelWrapper(
                settings.SegmentLogDirectory.Name ?? string.Empty,
                settings.SegmentLogDirectory.Path ?? string.Empty);
        }
        else
        {
            SegmentLogDirectory = new DirectoryModelWrapper("Segment Logs", string.Empty);
        }

        SegmentLogDirectory.PropertyChanged += (_, _) => OnPropertyChanged(string.Empty);
    }

    public LogMonitoringSettings ToLogMonitoringSettings() => new()
    {
        Enable = Enable,
        MasterLogDirectory = MasterLogDirectory.ToDirectoryModel(),
        SegmentLogDirectory = SegmentLogDirectory.ToDirectoryModel()
    };
}