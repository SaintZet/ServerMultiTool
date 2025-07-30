using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;
using ServerMultiTool.ViewModels.Contracts.Interfaces;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using ServerMultiTool.ViewModels.Pages.Pipeline.Managers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using static ServerMultiTool.ViewModels.Contracts.Delegates;

namespace ServerMultiTool.ViewModels.Pages.Pipeline;

public partial class PipelineViewModel : BaseViewModel, IPage, IGeneralInfoAware
{
    #region Observable Properties

    public string Title => PageNames.PipelinePage;

    [ObservableProperty] private GeneralInfoViewModel _generalInfo = null!;

    [ObservableProperty] private PipelineProfile? _selectedPipelineProfile;
    partial void OnSelectedPipelineProfileChanged(PipelineProfile? value)
    {
        if (value is null)
            return;

        PipelineProfileManager.UpdateProfile(value);
        _pipelineExecutor.UpdatePipelineOperations(value);
        _logManager.UpdateLogServices(value);
    }

    [ObservableProperty] private bool _isPipelineRunning;
    partial void OnIsPipelineRunningChanged(bool value)
    {
        StopPipelineCommand.NotifyCanExecuteChanged();
        ExecutePipelineCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Services

    private readonly PipelineProfileManager _profileManager;
    private readonly PipelineExecutionManager _pipelineExecutor;
    private readonly LogMonitoringManager _logManager;

    #endregion

    #region Collections

    [ObservableProperty] private List<PipelineProfile> _pipelineProfiles = [];
    public PipelineOperationCollection PipelineOperations => _pipelineExecutor.PipelineOperations;
    public ObservableCollection<LogEvent> AppLogMessages => _logManager.AppLogMessages;
    public ObservableCollection<LogEvent> MasterLogMessages => _logManager.MasterLogMessages;
    public ObservableCollection<LogEvent> SegmentLogMessages => _logManager.SegmentLogMessages;

    #endregion

    #region Constructor

    public PipelineViewModel()
    {
        _logManager = new LogMonitoringManager();

        _pipelineExecutor = new PipelineExecutionManager(_logManager);
        _pipelineExecutor.PipelineStateChanged += OnPipelineStateChanged;

        _profileManager = new PipelineProfileManager();
        _profileManager.ProfilesChanged += OnProfilesChanged;

        _profileManager.LoadProfiles(AppSettingsService.AppSettings.CurrentPipelineProfileName);

        PipelineProfiles = _profileManager.PipelineProfiles;
        SelectedPipelineProfile = _profileManager.SelectedProfile;
    }

    private void OnProfilesChanged(object? sender, System.EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _profileManager.LoadProfiles(SelectedPipelineProfile?.Name ?? string.Empty);
            OnPropertyChanged(nameof(PipelineProfiles));
            OnPropertyChanged(nameof(SelectedPipelineProfile));
        });
    }

    private void OnPipelineStateChanged(object? sender, bool isRunning)
    {
        IsPipelineRunning = isRunning;
    }

    #endregion

    #region Navigation

    public NavigateToSettingsDelegate? NavigateToSettingsAction { get; set; }

    [RelayCommand]
    private void NavigateToSettings()
    {
        if (SelectedPipelineProfile is null)
        {
            MessageBox.Show("Please select a pipeline profile first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        NavigateToSettingsAction?.Invoke(SettingsPageTabKeys.PipelineProfiles, SelectedPipelineProfile.Name);
    }

    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(IsPipelineRunning))]
    private void StopPipeline()
    {
        _pipelineExecutor.StopPipeline();
    }

    public bool CanExecutePipeline => GeneralInfo.CanChangeStates is true && IsPipelineRunning is false;

    [RelayCommand(CanExecute = nameof(CanExecutePipeline))]
    private async Task ExecutePipeline()
    {
        await _pipelineExecutor.ExecutePipeline(GeneralInfo);
    }

    #endregion
}