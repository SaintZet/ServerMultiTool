using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;
using System.Linq;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Controls;

public partial class GeneralInfoViewModel : BaseViewModel
{
    #region Observable Properties

    [ObservableProperty]
    private bool _canChangeStates = true;

    [ObservableProperty]
    private string? _currentGitBranch;

    [ObservableProperty]
    private DirectoryModel[] _solutionDirectories = [];

    [ObservableProperty]
    private DirectoryModel[] _httpDirectories = [];

    private DirectoryModel? _selectedSolutionDirectory;
    public DirectoryModel? SelectedSolutionDirectory
    {
        get => _selectedSolutionDirectory;
        set
        {
            if (value is null || value.Equals(_selectedSolutionDirectory))
                return;

            _selectedSolutionDirectory = value;

            OnUpdateSelectedSolutionDirectory(value).ConfigureAwait(false);
            OnPropertyChanged();
        }
    }

    private DirectoryModel? _selectedHttpDirectory;
    public DirectoryModel? SelectedHttpDirectory
    {
        get => _selectedHttpDirectory;
        set
        {
            if (value is null || value.Equals(_selectedHttpDirectory))
                return;

            _selectedHttpDirectory = value;

            OnUpdateSelectedHttpDirectory(value);
            OnPropertyChanged();
        }
    }

    #endregion

    #region Private Fields

    private readonly GitService _gitService = new();

    #endregion

    #region Constructor

    public GeneralInfoViewModel()
    {
        var appSettings = AppSettingsService.AppSettings;

        SolutionDirectories = appSettings.SolutionDirectories;
        HttpDirectories = appSettings.HttpDirectories;

        SelectedSolutionDirectory = SolutionDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentSolutionDirectoryName);
        SelectedHttpDirectory = HttpDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentHttpDirectoryName);
    }

    #endregion

    #region Methods

    public void UpdateData()
    {
        var appSettings = AppSettingsService.LoadOrInitialize();

        SolutionDirectories = appSettings.SolutionDirectories;
        HttpDirectories = appSettings.HttpDirectories;

        SelectedSolutionDirectory = SolutionDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentSolutionDirectoryName);
        SelectedHttpDirectory = HttpDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentHttpDirectoryName);

        OnPropertyChanged(nameof(SolutionDirectories));
        OnPropertyChanged(nameof(HttpDirectories));
    }

    private async Task OnUpdateSelectedSolutionDirectory(DirectoryModel value)
    {
        CurrentGitBranch = await _gitService.GetCurrentBranchName(value.Path);

        var appSettings = AppSettingsService.AppSettings;
        appSettings.CurrentSolutionDirectoryName = value.Name;

        AppSettingsService.SaveAppSettings(appSettings);
    }
    private static void OnUpdateSelectedHttpDirectory(DirectoryModel value)
    {
        var appSettings = AppSettingsService.AppSettings;
        appSettings.CurrentHttpDirectoryName = value.Name;

        AppSettingsService.SaveAppSettings(appSettings);
    }

    #endregion
}