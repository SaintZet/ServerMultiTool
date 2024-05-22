using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using CommunityToolkit.Mvvm.Input;

using ServerMultiTool.Model.Common.EventAggregator;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;

using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Data;

namespace ServerMultiTool.ViewModels.Pages;

public partial class PipelineViewModel : BaseViewModel
{
    private readonly GeneralInfoViewModel _generalInfo = null!;

    public GeneralInfoViewModel GeneralInfo
    {
        get => _generalInfo;
        init => SetProperty(ref _generalInfo, value);
    }
    
    public PipelineProfile[] PipelineProfiles { get; set; }
    
    private PipelineProfile _selectedPipelineProfile;
    public PipelineProfile SelectedPipelineProfile
    {
        get => _selectedPipelineProfile;
        set
        {
            if (value.Equals(_selectedPipelineProfile)) 
                return;
            
            _selectedPipelineProfile = value;
            
            OnUpdateSelectedPipelineProfile(value);
            
            OnPropertyChanged();
        }
    }

    private void OnUpdateSelectedPipelineProfile(PipelineProfile value)
    {
        UpdatePipelineOperations(value);
        //UpdateMasterLogService(value);
    }

    public ObservableCollection<PipelineOperationWrapper> PipelineOperations { get; } = new();

    public PipelineViewModel()
    {
        var appSettings = AppSettingsService.AppSettings;
        
        PipelineProfiles = PipelineProfilesService.PipelineProfiles;
        SelectedPipelineProfile = PipelineProfiles.FirstOrDefault(x => x.Name == appSettings.CurrentPipelineProfileName);
        
        ExecutePipelineCommand = new AsyncRelayCommand(StartPipeline);
        
        //_masterLogService.Subscribe<LogEvent>(AddNewLogEvent);
        GlobalEventAggregator.Subscribe<LogEvent>(AppLogMessages.Add);
        
        Application.Current.Exit += OnApplicationExit;
    }
    
    private async void OnApplicationExit(object sender, ExitEventArgs e)
    {
         // await StopMonitoringAsync();
    }

    public AsyncRelayCommand ExecutePipelineCommand { get; }
    
    private bool CanExecutePipeline => GeneralInfo.CanChangeStates;
    
    [RelayCommand(CanExecute = nameof(CanExecutePipeline))]
    private async Task StartPipeline()
    {
        GeneralInfo.CanChangeStates = false;
        
        foreach (var operation in PipelineOperations)
        {
            operation.UpdateSolutionDirectory(GeneralInfo.SelectedSolutionDirectory);
            operation.UpdateHttpDirectory(GeneralInfo.SelectedHttpDirectory);
            
            await operation.ExecuteAsync();
        }
    
        MessageBox.Show("Сборка завершена!");

        GeneralInfo.CanChangeStates = true;
    }

    private void UpdatePipelineOperations(PipelineProfile pipeline)
    {
        PipelineOperations.Clear();

        if (pipeline.GitSettings.Enable)
            PipelineOperations.Add(new( new GitService(pipeline.GitSettings), "Git"));
        
        if (pipeline.SettingsPerProject.Any(x => x.MsBuildSettings.Enable))
            PipelineOperations.Add(new(new MsBuildService(pipeline.SettingsPerProject), "MsBuild"));
        
        if (pipeline.SettingsPerProject.Any(x => x.DeliverySettings.Enable))
            PipelineOperations.Add(new( new DeliveryService(pipeline.SettingsPerProject), "Delivery"));

        if (pipeline.InternetInformationSettings.Enable)
            PipelineOperations.Add(new(new InternetInformationServices("/stop"), "IIS Stop"));

        if (pipeline.SqlExecutionSettings.Enable)
            PipelineOperations.Add(new( new SqlExecutionService(pipeline.SqlExecutionSettings), "SQL"));
        
        if (pipeline.InternetInformationSettings.Enable)
            PipelineOperations.Add(new( new InternetInformationServices("/start"), "IIS Start"));
        
        if (pipeline.WebBrowserSettings.Enable)
            PipelineOperations.Add(new( new WebBrowserService(pipeline.WebBrowserSettings), "Web"));
    }

    // private readonly LogMonitoringService _masterLogService = new();
    
    public ObservableCollection<LogEvent> AppLogMessages { get; } = new();
    
    // public ObservableCollection<LogEvent> MasterLogMessages { get; } = new();

    // private void UpdateMasterLogService(PipelineProfile profile)
    // {
    //     MasterLogMessages.Clear();
    //     _masterLogService.UpdateMonitoringSettings(profile.MonitorLogFilesSettings);
    // }
    //
    // private void AddNewLogEvent(LogEvent logEvent)
    // {
    //     if (MasterLogMessages.Contains(logEvent) is false)
    //         MasterLogMessages.Add(logEvent);
    // }
}