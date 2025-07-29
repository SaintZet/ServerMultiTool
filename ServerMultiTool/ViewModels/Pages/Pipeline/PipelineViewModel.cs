using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline.Contracts;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;
using ServerMultiTool.ViewModels.Contracts.Interfaces;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ServerMultiTool.ViewModels.Contracts.Delegates;

namespace ServerMultiTool.ViewModels.Pages.Pipeline;

public partial class PipelineViewModel : BaseViewModel, IPage, IGeneralInfoAware
{
    #region Observable Properties
    public string Title => "Pipeline";

    [ObservableProperty]
    private GeneralInfoViewModel _generalInfo = null!;

    [ObservableProperty]
    private PipelineProfile? _selectedPipelineProfile;

    partial void OnSelectedPipelineProfileChanged(PipelineProfile? value)
    {
        if (value is null)
            return;

        UpdateSettings(value);
        UpdatePipelineOperations(value);
        UpdateGameServerLogServices(value);
    }

    [ObservableProperty]
    private bool _isPipelineRunning;

    partial void OnIsPipelineRunningChanged(bool value)
    {
        StopPipelineCommand.NotifyCanExecuteChanged();
        ExecutePipelineCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Private Fields

    private readonly LogMonitoringService _masterLogService;
    private readonly LogMonitoringService _segmentLogService;

    private readonly Logger _logger;

    private CancellationTokenSource? _pipelineCancellationTokenSource;
    #endregion

    #region Collections

    public List<PipelineProfile> PipelineProfiles { get; set; } = [];
    public PipelineOperationCollection PipelineOperations { get; } = [];
    public ObservableCollection<LogEvent> AppLogMessages { get; } = [];
    public ObservableCollection<LogEvent> MasterLogMessages { get; } = [];
    public ObservableCollection<LogEvent> SegmentLogMessages { get; } = [];

    #endregion

    #region Constructor

    public PipelineViewModel()
    {
        _logger = new Logger(GetType());

        _masterLogService = new LogMonitoringService();
        _masterLogService.Subscribe<LogEvent>(AddNewMasterLogEvent);

        _segmentLogService = new LogMonitoringService();
        _segmentLogService.Subscribe<LogEvent>(AddNewSegmentLogEvent);

        GlobalEventAggregator.Instance.Subscribe<LogEvent>(AddNewGlobalLogEvent);

        LoadProfiles(AppSettingsService.AppSettings.CurrentPipelineProfileName);
        PipelineProfilesService.PipelineProfilesChanged += (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadProfiles(_selectedPipelineProfile!.Name);
            });
        };
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

    #region Load/Update Methods

    private void LoadProfiles(string selectedProfileName)
    {
        PipelineProfiles = PipelineProfilesService.PipelineProfiles;
        OnPropertyChanged(nameof(PipelineProfiles));

        var selectedProfile = PipelineProfiles.FirstOrDefault(x => x.Name == selectedProfileName);
        SelectedPipelineProfile = selectedProfile ?? PipelineProfiles.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedPipelineProfile));
    }

    private static void UpdateSettings(PipelineProfile value)
    {
        var appSettings = AppSettingsService.AppSettings;
        appSettings.CurrentPipelineProfileName = value.Name;
        AppSettingsService.SaveAppSettings(appSettings);
    }

    private void UpdatePipelineOperations(PipelineProfile pipeline)
    {
        PipelineOperations.Clear();

        if (pipeline.GitSettings.Enable)
            PipelineOperations.Add(new(new GitService(pipeline.GitSettings), "Git"));

        if (pipeline.SettingsPerProject.Any(x => x.MsBuildSettings.Enable))
            PipelineOperations.Add(new(new MsBuildService(pipeline.SettingsPerProject), "MsBuild"));

        if (pipeline.InternetInformationSettings.Enable)
            PipelineOperations.Add(new(new InternetInformationServices("/stop", pipeline.InternetInformationSettings), "IIS Stop"));

        if (pipeline.SettingsPerProject.Any(x => x.DeliverySettings.EnableCustomDelivery || x.DeliverySettings.EnableDeliveryBin))
            PipelineOperations.Add(new(new DeliveryService(pipeline.SettingsPerProject), "Delivery"));

        if (pipeline.SqlExecutionSettings.Enable)
            PipelineOperations.Add(new(new SqlExecutionService(pipeline.SqlExecutionSettings), "SQL"));

        if (pipeline.InternetInformationSettings.Enable)
            PipelineOperations.Add(new(new InternetInformationServices("/start", pipeline.InternetInformationSettings), "IIS Start"));

        if (pipeline.WebBrowserSettings.Enable)
            PipelineOperations.Add(new(new WebBrowserService(pipeline.WebBrowserSettings), "Web"));

        if (pipeline.HttpMonitoringSettings.Enable)
            PipelineOperations.Add(new(new HttpMonitoringService(pipeline.HttpMonitoringSettings), "Http"));
    }

    private void UpdateGameServerLogServices(PipelineProfile profile)
    {
        var settings = profile.MonitorLogFilesSettings;

        _ = _masterLogService.UpdateSettings(settings.Enable, settings.MasterLogDirectory);
        _ = _segmentLogService.UpdateSettings(settings.Enable, settings.SegmentLogDirectory);

        Application.Current.Dispatcher.Invoke(() =>
        {
            MasterLogMessages.Clear();
            SegmentLogMessages.Clear();
        });
    }

    #endregion

    #region Commands

    private bool CanStopPipeline => _isPipelineRunning;

    [RelayCommand(CanExecute = nameof(CanStopPipeline))]
    private void StopPipeline()
    {
        _pipelineCancellationTokenSource?.Cancel();
    }

    public bool CanExecutePipeline => GeneralInfo?.CanChangeStates is true && _isPipelineRunning is false;

    [RelayCommand(CanExecute = nameof(CanExecutePipeline))]
    private async Task ExecutePipeline()
    {
        try
        {
            await StartPipelineExecution();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInfoWithPublish("Pipeline execution was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogErrorWithPublish($"Error during pipeline execution: {ex.Message}");
        }
        finally
        {
            CompletePipelineExecution();
        }
    }

    private async Task StartPipelineExecution()
    {
        IsPipelineRunning = true;
        GeneralInfo.CanChangeStates = false;

        AppLogMessages.Clear();
        PipelineOperations.ClearStatuses();

        _pipelineCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _pipelineCancellationTokenSource.Token;

        await ExecuteOperationsSequentially(cancellationToken);
    }

    private async Task ExecuteOperationsSequentially(CancellationToken cancellationToken)
    {
        foreach (var operation in PipelineOperations)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                PipelineOperations.CancelWaitingOperations();
                break;
            }

            operation.OperationStarted();

            if (GeneralInfo.SelectedSolutionDirectory is not null)
                operation.UpdateSolutionDirectory(GeneralInfo.SelectedSolutionDirectory);

            if (GeneralInfo.SelectedHttpDirectory is not null)
                operation.UpdateHttpDirectory(GeneralInfo.SelectedHttpDirectory);

            try
            {
                var result = await operation.ExecuteAsync(cancellationToken);

                if (result == OperationResult.Cancelled)
                {
                    PipelineOperations.CancelWaitingOperations();
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                operation.CancelOperation();

                PipelineOperations.CancelWaitingOperations();
                break;
            }
        }
    }

    private void CompletePipelineExecution()
    {
        IsPipelineRunning = false;
        GeneralInfo.CanChangeStates = true;
    }

    #endregion

    #region Log Event Handlers

    private void AddNewMasterLogEvent(LogEvent logEvent) =>
        AddNewLogEvent(logEvent, MasterLogMessages);

    private void AddNewSegmentLogEvent(LogEvent logEvent) =>
        AddNewLogEvent(logEvent, SegmentLogMessages);

    private void AddNewGlobalLogEvent(LogEvent logEvent) =>
        AddNewLogEvent(logEvent, AppLogMessages);

    private static void AddNewLogEvent(LogEvent logEvent, ObservableCollection<LogEvent> targetCollection)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (targetCollection.Contains(logEvent))
                return;

            targetCollection.Add(logEvent);
        });
    }

    #endregion
}