using System.Linq;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Controls;

public class GeneralInfoViewModel : BaseViewModel
{
    private readonly GitService _gitService = new();
    
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
        set => SetProperty(ref _canChangeStates, value);
    }

    public GeneralInfoViewModel()
    {
        var appSettings = AppSettingsService.AppSettings;
        
        SolutionDirectories = appSettings.SolutionDirectories;
        HttpDirectories = appSettings.HttpDirectories;
        
        SelectedSolutionDirectory = SolutionDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentSolutionDirectoryName);
        SelectedHttpDirectory = HttpDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentHttpDirectoryName);
    }
}