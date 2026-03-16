using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Common.Interfaces;
using ServerMultiTool.ViewModels.Components.GeneralInfo;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Management;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System;
using System.Collections;
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

    [ObservableProperty] private GeneralInfoViewModel _generalInfo;

    [ObservableProperty] private PipelineProfileWrapper? _selectedPipelineProfile;

    partial void OnSelectedPipelineProfileChanged(PipelineProfileWrapper? value)
    {
        if (value is null)
            return;

        var settings = _appSettingsService.Get();
        settings.CurrentPipelineProfileName = value.Name;
        _appSettingsService.Save(settings);

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

    #region Services

    private readonly IAppSettingsService _appSettingsService;
    private readonly IPipelineProfilesService _pipelineProfilesService;

    #endregion

    #region Collections

    [ObservableProperty] ObservableCollection<PipelineProfileWrapper> _pipelineProfiles = [];
    public PipelineStepsCollection PipelineSteps => _pipelineExecutor.PipelineSteps;
    public ObservableCollection<LogEvent> AppLogMessages => _logManager.AppLogMessages;
    public ObservableCollection<LogEvent> MasterLogMessages => _logManager.MasterLogMessages;
    public ObservableCollection<LogEvent> SegmentLogMessages => _logManager.SegmentLogMessages;

    #endregion

    #region Constructor

    public PipelineViewModel(
        IAppSettingsService appSettingsService,
        IPipelineProfilesService pipelineProfilesService,
        GeneralInfoViewModel generalInfo)
    {
        _appSettingsService = appSettingsService;
        _pipelineProfilesService = pipelineProfilesService;
        _generalInfo = generalInfo;

        _logManager = new GsLogMonitorManager();

        _pipelineExecutor = new PipelineExecuteManager(_logManager);
        _pipelineExecutor.PipelineStateChanged += (sender, isRunning) => { IsPipelineRunning = isRunning; };

        LoadProfiles();

        _pipelineProfilesService.ProfilesChanged += (_, _) => Application.Current.Dispatcher.Invoke(LoadProfiles);
    }

    private void LoadProfiles()
    {
        var settings = _appSettingsService.Get();
        var selectedName = settings.CurrentPipelineProfileName;

        PipelineProfiles.Clear();

        foreach (var profile in _pipelineProfilesService.GetAll())
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

    [RelayCommand]
    private static void CopyLogMessage(LogEvent? logEvent)
    {
        if (logEvent is null)
            return;

        var fullMessage = $"[{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss}] {logEvent.Sender}: {logEvent.Message.BaseMessage}";
        
        if (!string.IsNullOrEmpty(logEvent.Message.ExtendedMessage)) 
            fullMessage += $"{Environment.NewLine}{logEvent.Message.ExtendedMessage}";
        
        SafeSetClipboardText(fullMessage);
    }

    [RelayCommand]
    private static void CopySelectedLogMessages(IList? selectedItems)
    {
        if (selectedItems is null || selectedItems.Count == 0)
            return;

        var logEvents = selectedItems.Cast<LogEvent>().OrderBy(l => l.Timestamp);
        var sb = new System.Text.StringBuilder();

        foreach (var logEvent in logEvents)
        {
            sb.Append($"[{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss}] {logEvent.Sender}: {logEvent.Message.BaseMessage}");
            
            if (!string.IsNullOrEmpty(logEvent.Message.ExtendedMessage)) 
                sb.Append($"{Environment.NewLine}{logEvent.Message.ExtendedMessage}");
            
            sb.AppendLine();
        }

        SafeSetClipboardText(sb.ToString());
    }
    
    private static void SafeSetClipboardText(string text)
    {
        for (var i = 0; i < 10; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return;
            }
            catch (System.Runtime.InteropServices.COMException ex) when ((uint)ex.ErrorCode == 0x800401D0)
            {
                if (i == 9) throw;
                System.Threading.Thread.Sleep(50);
            }
        }
    }

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