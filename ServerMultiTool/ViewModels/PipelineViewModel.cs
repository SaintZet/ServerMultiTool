using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
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
using ServerMultiTool.ViewModels.Data;

namespace ServerMultiTool.ViewModels;

public partial class PipelineViewModel : BaseViewModel
{
    #region Properties
    
    public DirectoryModel[] SolutionDirectories { get; set; }

    private DirectoryModel _selectedSolutionDirectory;
    public DirectoryModel SelectedSolutionDirectory
    {
        get => _selectedSolutionDirectory;
        set
        {
            if (value.Equals(_selectedSolutionDirectory)) 
                return;
            
            _selectedSolutionDirectory = value;
            OnUpdateSelectedSolutionDirectory(value);
            OnPropertyChanged();
        }
    }
    
    private async void OnUpdateSelectedSolutionDirectory(DirectoryModel value)
    {
        CurrentGitBranch = await _gitService.GetCurrentBranchName(value.Path);
    }

    public DirectoryModel[] HttpDirectories { get; set; }
    
    private DirectoryModel _selectedHttpDirectory;
    public DirectoryModel SelectedHttpDirectory
    {
        get => _selectedHttpDirectory;
        set
        {
            if (value.Equals(_selectedHttpDirectory)) 
                return;
            
            _selectedHttpDirectory = value;
            OnPropertyChanged();
        }
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

    private string? _currentGitBranch;
    public string? CurrentGitBranch
    {
        get => _currentGitBranch;
        private set => SetProperty(ref _currentGitBranch, value);
    }

    private bool _canChangeStates = true;
    public bool CanChangeStates
    {
        get => _canChangeStates;
        private set => SetProperty(ref _canChangeStates, value);
    }

    #endregion

    #region Constructor

    public ObservableCollection<PipelineOperationWrapper> PipelineOperations { get; } = new();

    private readonly GitService _gitService = new();
    public PipelineViewModel()
    {
        var appSettings = AppSettingsService.AppSettings;
        
        SolutionDirectories = appSettings.SolutionDirectories;
        HttpDirectories = appSettings.HttpDirectories;
        PipelineProfiles = PipelineProfilesService.PipelineProfiles;
        
        SelectedSolutionDirectory = SolutionDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentSolutionDirectoryName);
        SelectedHttpDirectory = HttpDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentHttpDirectoryName);
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

    #endregion

    #region Commands

    public AsyncRelayCommand ExecutePipelineCommand { get; }

    [RelayCommand(CanExecute = nameof(CanChangeStates))]
    private async Task StartPipeline()
    {
        CanChangeStates = false;
        
        foreach (var operation in PipelineOperations)
        {
            operation.UpdateSolutionDirectory(SelectedSolutionDirectory);
            operation.UpdateHttpDirectory(SelectedHttpDirectory);
            
            await operation.ExecuteAsync();
        }

        MessageBox.Show("Сборка завершена!");

        CanChangeStates = true;
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

    #endregion

    #region Logs

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
    
    #endregion
}