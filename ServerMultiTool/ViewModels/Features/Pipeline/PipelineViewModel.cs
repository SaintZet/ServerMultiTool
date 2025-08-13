using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Common.Interfaces;
using ServerMultiTool.ViewModels.Components.GeneralInfo;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Management;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System.Collections.ObjectModel;
using System.Linq;
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

        var settings = App.FileAppSettingsService.Get();
        settings.CurrentPipelineProfileName = value.Name;
        App.FileAppSettingsService.Save(settings);

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

    #region Managers

    private readonly PipelineExecuteManager _pipelineExecutor;
    private readonly GsLogMonitorManager _logManager;

    #endregion

    #region Collections

    [ObservableProperty] ObservableCollection<PipelineProfileWrapper> _pipelineProfiles = [];
    public PipelineStepsCollection PipelineSteps => _pipelineExecutor.PipelineSteps;
    public ObservableCollection<LogEvent> AppLogMessages => _logManager.AppLogMessages;
    public ObservableCollection<LogEvent> MasterLogMessages => _logManager.MasterLogMessages;
    public ObservableCollection<LogEvent> SegmentLogMessages => _logManager.SegmentLogMessages;

    #endregion

    #region Constructor

    public PipelineViewModel()
    {
        _logManager = new GsLogMonitorManager();

        _pipelineExecutor = new PipelineExecuteManager(_logManager);
        _pipelineExecutor.PipelineStateChanged += (sender, isRunning) => { IsPipelineRunning = isRunning; };

        LoadProfiles();

        App.FilePipelineProfilesService.ProfilesChanged += (_, _) => Application.Current.Dispatcher.Invoke(LoadProfiles);
    }

    private void LoadProfiles()
    {
        var settings = App.FileAppSettingsService.Get();
        var selectedName = settings.CurrentPipelineProfileName;

        PipelineProfiles.Clear();

        foreach (var profile in App.FilePipelineProfilesService.GetAll())
        {
            var wrapper = new PipelineProfileWrapper(profile);
            PipelineProfiles.Add(wrapper);
        }

        RestoreSelectedPipelineProfile(selectedName);
    }

    private void RestoreSelectedPipelineProfile(string? selectedName)
    {
        if (!string.IsNullOrEmpty(selectedName))
        {
            SelectedPipelineProfile = PipelineProfiles.FirstOrDefault(p => p.Name == selectedName) ?? PipelineProfiles.FirstOrDefault();
        }
        else if (PipelineProfiles.Any())
        {
            SelectedPipelineProfile = PipelineProfiles.First();
        }
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