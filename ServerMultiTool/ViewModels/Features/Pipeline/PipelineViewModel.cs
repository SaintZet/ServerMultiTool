using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Domain.Common.Logs;
using ServerMultiTool.Model.Services.Settings;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Common.Interfaces;
using ServerMultiTool.ViewModels.Components.GeneralInfo;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Management;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using static ServerMultiTool.ViewModels.Common.Delegates;

namespace ServerMultiTool.ViewModels.Pages.Pipeline;

public partial class PipelineViewModel : BaseViewModel, IPage
{
    #region Observable Properties

    public string Title => PageNames.PipelinePage;

    [ObservableProperty] private GeneralInfoViewModel _generalInfo = null!;

    [ObservableProperty] private PipelineProfileWrapper? _selectedPipelineProfile;
    partial void OnSelectedPipelineProfileChanged(PipelineProfileWrapper? value)
    {
        if (value is null)
            return;

        _profileManager.UpdateProfile(value);

        _pipelineExecutor.UpdateOperations(value);
        OnPropertyChanged(nameof(PipelineSteps));

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
    private readonly PipelineExecuteManager _pipelineExecutor;
    private readonly GsLogMonitor _logManager;

    #endregion

    #region Collections

    [ObservableProperty] List<PipelineProfileWrapper> _pipelineProfiles = [];
    public PipelineStepsCollection PipelineSteps => _pipelineExecutor.PipelineSteps;
    public ObservableCollection<LogEvent> AppLogMessages => _logManager.AppLogMessages;
    public ObservableCollection<LogEvent> MasterLogMessages => _logManager.MasterLogMessages;
    public ObservableCollection<LogEvent> SegmentLogMessages => _logManager.SegmentLogMessages;

    #endregion

    #region Constructor

    public PipelineViewModel()
    {
        _logManager = new GsLogMonitor();

        _pipelineExecutor = new PipelineExecuteManager(_logManager);
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