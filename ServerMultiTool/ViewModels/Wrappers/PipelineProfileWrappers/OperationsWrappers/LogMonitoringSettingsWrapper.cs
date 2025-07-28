using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Pages.Settings.Wrappers;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class LogMonitoringSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty] private bool _enable;
    [ObservableProperty] private DirectoryModelWrapper _logDirectory;

    public LogMonitoringSettingsWrapper(LogMonitoringSettings settings)
    {
        Enable = settings.Enable;
        LogDirectory = new DirectoryModelWrapper(settings.MasterLogDirectory!.Name ?? string.Empty, settings.MasterLogDirectory.Path ?? string.Empty);
        LogDirectory.PropertyChanged += (_, _) => OnPropertyChanged(string.Empty);
    }

    public LogMonitoringSettings ToLogMonitoringSettings()
    {
        return new LogMonitoringSettings
        {
            Enable = Enable,
            MasterLogDirectory = LogDirectory.ToDirectoryModel()
        };
    }

    partial void OnEnableChanged(bool value)
    {
        OnPropertyChanged(string.Empty);
    }
}