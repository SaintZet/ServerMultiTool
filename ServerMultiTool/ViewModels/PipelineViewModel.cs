using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;
using ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Data;

namespace ServerMultiTool.ViewModels;

public partial class PipelineViewModel : BaseViewModel
{
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
            
            OnPropertyChanged();
        }
    }

    public DirectoryModel[] SolutionDirectories { get; set; }
    public DirectoryModel[] HttpDirectories { get; set; }
    public PipelineProfile[] PipelineProfiles { get; set; }
    
    private MilestoneContainer _milestones = null!;
    public MilestoneContainer Milestones
    {
        get => _milestones;
        private set
        {
            _milestones = value;
            OnPropertyChanged();
        }
    }
    
    private string _currentGitBranch = null!;
    public string CurrentGitBranch
    {
        get => _currentGitBranch;
        private set
        {
            _currentGitBranch = value;
            OnPropertyChanged();
        }
    }

    private bool _canChangeStates = true;
    public bool CanChangeStates
    {
        get => _canChangeStates;
        set
        {
            if (value == _canChangeStates) return;
            _canChangeStates = value;
            OnPropertyChanged();
        }
    }
    
    private readonly GitService _gitService;
    private readonly MsBuildService _msBuildService;
    private readonly DeliveryService _deliveryService;
    
    public PipelineViewModel()
    {
        SolutionDirectories = AppSettingsService.AppSettings.SolutionDirectories;
        HttpDirectories = AppSettingsService.AppSettings.HttpDirectories;
        PipelineProfiles = PipelineProfilesService.PipelineProfiles;
        
        _selectedSolutionDirectory = SolutionDirectories.FirstOrDefault(x => x.Id == AppSettingsService.AppSettings.CurrentSolutionDirectoryId);
        _selectedHttpDirectory = HttpDirectories.FirstOrDefault(x => x.Id == AppSettingsService.AppSettings.CurrentHttpDirectoryId);
        _selectedPipelineProfile = PipelineProfiles.FirstOrDefault(x => x.Id == AppSettingsService.AppSettings.LastPipelineProfileId);
        
        _gitService = new GitService(_selectedSolutionDirectory.Path);
        _msBuildService = new MsBuildService(_selectedSolutionDirectory.Path);
        _deliveryService = new DeliveryService(_selectedSolutionDirectory.Path, _selectedHttpDirectory.Path);
        
        ExecutePipelineCommand = new AsyncRelayCommand(StartPipeline);

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        CurrentGitBranch = await _gitService.GetCurrentBranchName();
        UpdateMilestoneContainer(_selectedPipelineProfile);
    }

    private async Task<bool> ExecuteGitOperations() => await _gitService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteMsBuildOperations() => await _msBuildService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteDeliveryOperations() => await _deliveryService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteSqlOperations() => await SqlExecutionService.ExecuteAsync(SelectedPipelineProfile);
    private async Task<bool> ExecuteWebBrowserOperations() => await WebBrowserService.ExecuteAsync(SelectedPipelineProfile);

    private void UpdateMilestoneContainer(PipelineProfile profile)
    {
        var container = new MilestoneContainer();

        if (profile.GitSettings.Enable)
            container.Add(new("Git", ExecuteGitOperations));

        if (profile.SettingsPerProject.Any(x => x.MsBuildSettings.Enable))
            container.Add(new("MsBuild", ExecuteMsBuildOperations));
        
        if (profile.SettingsPerProject.Any(x => x.DeliverySettings.DeliveryBin || x.DeliverySettings.DeliveryDirectory?.Length > 0))
            container.Add(new("Delivery", ExecuteDeliveryOperations));
        
        container.Add(new("IIS Stop", async () => await InternetInformationServices.StopAsync()));
        
        if (profile.SqlExecutionSettings.Enable)
            container.Add(new("Sql", ExecuteSqlOperations));

        container.Add(new("IIS Start", async () => await InternetInformationServices.StartAsync()));
        
        if (profile.WebBrowserSettings.Enable)
            container.Add(new("Web Browser", ExecuteWebBrowserOperations));

        Milestones = container;
    }
    
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
}