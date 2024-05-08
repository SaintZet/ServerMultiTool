using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using log4net;
using ServerMultiTool.Model;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;
using ServerMultiTool.Model.Logs;
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
            _msBuildService.SolutionDirectory = value.Path;
            _deliveryService.SolutionDirectory = value.Path;
            _gitService.SolutionDirectory = value.Path;
            
            CurrentGitBranch = _gitService.GetCurrentBranchName().Result;
            
            OnPropertyChanged();
        }
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
            _deliveryService.HttpDirectory = value.Path;
            
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
            
            UpdateMilestoneContainer(value);
            UpdateMasterLogService(value);
            
            OnPropertyChanged();
        }
    }

    private string _currentGitBranch = null!;
    public string CurrentGitBranch
    {
        get => _currentGitBranch;
        private set => SetProperty(ref _currentGitBranch, value);
    }

    private bool _canChangeStates = true;
    public bool CanChangeStates
    {
        get => _canChangeStates;
        set => SetProperty(ref _canChangeStates, value);
    }

    #endregion

    #region Services

    private readonly GitService _gitService;
    private readonly MsBuildService _msBuildService;
    private readonly DeliveryService _deliveryService;
    private readonly LogMonitoringService _masterLogService;
    private readonly SqlExecutionService _sqlService;
    private readonly WebBrowserService _webBrowserService;
    private readonly InternetInformationServices _iisService;
    
    #endregion

    #region Constructor
    
    private static readonly ILog Log = LogManager.GetLogger(nameof(PipelineViewModel));

    public PipelineViewModel()
    {
        var appSettings = AppSettingsService.AppSettings;
        
        SolutionDirectories = appSettings.SolutionDirectories;
        HttpDirectories = appSettings.HttpDirectories;
        PipelineProfiles = PipelineProfilesService.PipelineProfiles;
        
        _selectedSolutionDirectory = SolutionDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentSolutionDirectoryName);
        _selectedHttpDirectory = HttpDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentHttpDirectoryName);
        SelectedPipelineProfile = PipelineProfiles.FirstOrDefault(x => x.Name == appSettings.CurrentPipelineProfileName);
        
        _gitService = new GitService(_selectedSolutionDirectory.Path);
        _msBuildService = new MsBuildService(_selectedSolutionDirectory.Path);
        _deliveryService = new DeliveryService(_selectedSolutionDirectory.Path, _selectedHttpDirectory.Path);
        _sqlService = new SqlExecutionService();
        _webBrowserService = new WebBrowserService();
        _iisService = new InternetInformationServices();

        _masterLogService = new LogMonitoringService();
        _masterLogService.Subscribe<LogEvent>(AddNewLogEvent);
        
        EventAggregator.Subscribe<LogEvent>(AppLogMessages.Add);
        
        // Application.Current.Exit += OnApplicationExit;
        
        ExecutePipelineCommand = new AsyncRelayCommand(StartPipeline);

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        CurrentGitBranch = await _gitService.GetCurrentBranchName();
        
        await UpdateMasterLogService(_selectedPipelineProfile);
        UpdateMilestoneContainer(_selectedPipelineProfile);
    }

    // private async void OnApplicationExit(object sender, ExitEventArgs e)
    // {
    //      await StopMonitoringAsync();
    // }

    #endregion

    #region Commands

    public AsyncRelayCommand ExecutePipelineCommand { get; }

    [RelayCommand(CanExecute = nameof(CanChangeStates))]
    private async Task StartPipeline()
    {
        CanChangeStates = false;
    
        Milestones.ResetAllIndicators();
        await Milestones.StartExecute();

        MessageBox.Show("Сборка завершена!");

        CanChangeStates = true;
    }

    #endregion
    
    #region Milestones

    private MilestoneContainer _milestones = null!;

    public MilestoneContainer Milestones
    {
        get => _milestones;
        private set => SetProperty(ref _milestones, value);
    }

    private void UpdateMilestoneContainer(PipelineProfile profile)
    {
        var container = new MilestoneContainer();

        if (profile.GitSettings.Enable)
            container.Add(new("Git", ExecuteGitOperations));

        if (profile.SettingsPerProject.Any(x => x.MsBuildSettings.Enable))
            container.Add(new("MsBuild", ExecuteMsBuildOperations));
        
        if (profile.SettingsPerProject.Any(x => x.DeliverySettings.DeliveryBin || x.DeliverySettings.DeliveryDirectory?.Length > 0))
            container.Add(new("Delivery", ExecuteDeliveryOperations));
        
        container.Add(new("IIS Stop", async () => await _iisService.StopAsync()));
        
        if (profile.SqlExecutionSettings.Enable)
            container.Add(new("Sql", ExecuteSqlOperations));

        container.Add(new("IIS Start", async () => await _iisService.StartAsync()));
        
        if (profile.WebBrowserSettings.Enable)
            container.Add(new("Web Browser", ExecuteWebBrowserOperations));

        Milestones = container;
    }
    
    private async Task<bool> ExecuteGitOperations() => await _gitService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteMsBuildOperations() => await _msBuildService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteDeliveryOperations() => await _deliveryService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteSqlOperations() => await _sqlService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteWebBrowserOperations() => await _webBrowserService.ExecuteAsync(SelectedPipelineProfile);

    #endregion

    #region Logs
    
    public ObservableCollection<LogEvent> AppLogMessages { get; } = new();
    public ObservableCollection<LogEvent> MasterLogMessages { get; } = new();

    private async Task UpdateMasterLogService(PipelineProfile profile)
    {
        MasterLogMessages.Clear();
        await _masterLogService.UpdateMonitoringSettings(profile.MonitorLogFilesSettings);
    }

    private void AddNewLogEvent(LogEvent logEvent)
    {
        if (MasterLogMessages.Contains(logEvent) is false)
            MasterLogMessages.Add(logEvent);
    }
    
    #endregion
}