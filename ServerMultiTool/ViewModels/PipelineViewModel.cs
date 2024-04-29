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

namespace ServerMultiTool.ViewModels;

public partial class PipelineViewModel : BaseViewModel
{
    public DirectoryModel[] SolutionDirectories { get; set; }
    public DirectoryModel SelectedSolutionDirectory { get; set; }
    
    public DirectoryModel[] HttpDirectories { get; set; }
    public DirectoryModel SelectedHttpDirectory { get; set; }
    
    public PipelineProfile[] PipelineProfiles { get; set; }
    public PipelineProfile SelectedPipelineProfile { get; set; }
    
    
    private string _currentGitBranch;
    public string CurrentGitBranch
    {
        get => _currentGitBranch;
        private set => SetProperty(ref _currentGitBranch, value);
    }

    private static bool _deployInProcess;
    private GitService _gitService;
        
    public PipelineViewModel()
    {
        SolutionDirectories = AppSettingsService.AppSettings.SolutionDirectories;
        SelectedSolutionDirectory = SolutionDirectories[0];
        
        HttpDirectories = AppSettingsService.AppSettings.HttpDirectories;
        SelectedHttpDirectory = HttpDirectories[0];
        
        PipelineProfiles = PipelineProfilesService.PipelineProfiles;
        SelectedPipelineProfile = PipelineProfiles[0];
        
        _gitService = new GitService(SelectedSolutionDirectory.Path);
        
        DeployCommand = new AsyncRelayCommand(ExecuteDeployCommand);
        
        LoadDataAsync();
    }
    
    private async Task LoadDataAsync()
    {
        CurrentGitBranch = await _gitService.GetCurrentBranchName();
    }

    public AsyncRelayCommand DeployCommand { get; }

    [RelayCommand(CanExecute = nameof(CanExecuteDeploy))]
    private async Task ExecuteDeployCommand()
    {
        _deployInProcess = true;
        
        var solutionDirectory = SelectedSolutionDirectory.Path;
        var httpDirectory = SelectedHttpDirectory.Path;
        var pipeline = SelectedPipelineProfile;
        
        await _gitService.ExecuteAsync(pipeline);
        await new MsBuildService(solutionDirectory).ExecuteAsync(pipeline);
        await new DeliveryService(solutionDirectory, httpDirectory).ExecuteAsync(pipeline);
        await InternetInformationServices.StopAsync();
        await SqlExecutionService.ExecuteAsync(pipeline);
        await InternetInformationServices.StartAsync();
        await WebBrowserService.ExecuteAsync(pipeline);

        MessageBox.Show("Сборка завершена!");

        _deployInProcess = false;
    }

    private static bool CanExecuteDeploy() =>
        _deployInProcess is not true;
}