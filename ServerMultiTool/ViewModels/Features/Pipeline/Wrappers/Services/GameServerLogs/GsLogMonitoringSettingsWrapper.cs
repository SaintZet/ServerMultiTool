using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.Services.GameServerLogs;
using ServerMultiTool.ViewModels.Features.Settings.Wrappers;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Services.GameServerLogs;

public partial class GsLogMonitoringSettingsWrapper : ObservableObject
{
    [ObservableProperty] private bool _enable;

    [ObservableProperty] DirectoryModelWrapper _masterLogDirectory;

    [ObservableProperty] DirectoryModelWrapper _segmentLogDirectory;

    readonly GsLogMonitoringSettings _settings;

    public GsLogMonitoringSettingsWrapper(GsLogMonitoringSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings), "Game Server Log Monitoring Settings cannot be null.");

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

    public GsLogMonitoringSettings ToOriginal()
    {
        _settings.Enable = Enable;
        _settings.MasterLogDirectory = MasterLogDirectory.ToOriginal();
        _settings.SegmentLogDirectory = SegmentLogDirectory.ToOriginal();

        return _settings;
    }
}
