using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ServerMultiTool.ViewModels.Pages.Pipeline
{
    public partial class PipelineViewModel : BaseViewModel
    {
        private readonly GeneralInfoViewModel _generalInfo = null!;
        public GeneralInfoViewModel GeneralInfo
        {
            get => _generalInfo;
            init => SetProperty(ref _generalInfo, value);
        }

        public List<PipelineProfile> PipelineProfiles { get; set; } = [];

        private PipelineProfile? _selectedPipelineProfile;
        public PipelineProfile? SelectedPipelineProfile
        {
            get => _selectedPipelineProfile;
            set
            {
                if (value is null || value.Equals(_selectedPipelineProfile))
                    return;

                _selectedPipelineProfile = value;

                UpdateSettings(value);
                UpdatePipelineOperations(value);
                UpdateMasterLogService(value);

                OnPropertyChanged();
            }
        }

        public PipelineOperationCollection PipelineOperations { get; } = [];

        public ObservableCollection<LogEvent> AppLogMessages { get; } = [];
        public ObservableCollection<LogEvent> MasterLogMessages { get; } = [];

        private readonly LogMonitoringService _masterLogService;

        private CancellationTokenSource? _pipelineCancellationTokenSource;

        private bool _isPipelineRunning;
        public bool IsPipelineRunning
        {
            get => _isPipelineRunning;
            set
            {
                if (!SetProperty(ref _isPipelineRunning, value))
                    return;

                StopPipelineCommand.NotifyCanExecuteChanged();
                ExecutePipelineCommand.NotifyCanExecuteChanged();
            }
        }

        public PipelineViewModel()
        {
            _masterLogService = new LogMonitoringService();
            _masterLogService.Subscribe<LogEvent>(AddNewMasterLogEvent);

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

        private void LoadProfiles(string selectedProfileName)
        {
            PipelineProfiles = PipelineProfilesService.PipelineProfiles;
            OnPropertyChanged(nameof(PipelineProfiles));

            var selectedProfile = PipelineProfiles.FirstOrDefault(x => x.Name == selectedProfileName);
            SelectedPipelineProfile = selectedProfile ?? PipelineProfiles.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedPipelineProfile));
        }
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
            GeneralInfo.CanChangeStates = false;
            IsPipelineRunning = true;

            _pipelineCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _pipelineCancellationTokenSource.Token;

            PipelineOperations.ClearStatuses();

            try
            {
                foreach (var operation in PipelineOperations)
                {
                    operation.UpdateSolutionDirectory(GeneralInfo.SelectedSolutionDirectory);
                    operation.UpdateHttpDirectory(GeneralInfo.SelectedHttpDirectory);

                    await operation.ExecuteAsync(cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                AppLogMessages.Clear();
                PipelineOperations.ClearStatuses();
            }
            finally
            {
                IsPipelineRunning = false;
                GeneralInfo.CanChangeStates = true;
            }
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
                PipelineOperations.Add(new(new InternetInformationServices("/stop"), "IIS Stop"));

            if (pipeline.SettingsPerProject.Any(x => x.DeliverySettings.EnableCustomDelivery || x.DeliverySettings.EnableDeliveryBin))
                PipelineOperations.Add(new(new DeliveryService(pipeline.SettingsPerProject), "Delivery"));

            if (pipeline.SqlExecutionSettings.Enable)
                PipelineOperations.Add(new(new SqlExecutionService(pipeline.SqlExecutionSettings), "SQL"));

            if (pipeline.InternetInformationSettings.Enable)
                PipelineOperations.Add(new(new InternetInformationServices("/start"), "IIS Start"));

            if (pipeline.WebBrowserSettings.Enable)
                PipelineOperations.Add(new(new WebBrowserService(pipeline.WebBrowserSettings), "Web"));

            if (pipeline.HttpMonitoringSettings.Enable)
                PipelineOperations.Add(new(new HttpMonitoringService(pipeline.HttpMonitoringSettings), "Http"));
        }

        private void UpdateMasterLogService(PipelineProfile profile)
        {
            _ = _masterLogService.UpdateSettings(profile.MonitorLogFilesSettings);

            Application.Current.Dispatcher.Invoke(() =>
            {
                MasterLogMessages.Clear();
            });
        }

        private void AddNewMasterLogEvent(LogEvent logEvent)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (MasterLogMessages.Contains(logEvent))
                    return;

                MasterLogMessages.Add(logEvent);
            });
        }

        private void AddNewGlobalLogEvent(LogEvent logEvent)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AppLogMessages.Add(logEvent);
            });
        }
    }
}