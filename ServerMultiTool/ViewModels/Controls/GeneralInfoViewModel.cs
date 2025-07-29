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

    [ObservableProperty] private bool _canChangeStates = true;

    [ObservableProperty] private string? _currentGitBranch;

    [ObservableProperty] private DirectoryModel[] _solutionDirectories = [];
    [ObservableProperty] private DirectoryModel? _selectedSolutionDirectory;

    [ObservableProperty] private DirectoryModel[] _httpDirectories = [];
    [ObservableProperty] private DirectoryModel? _selectedHttpDirectory;

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

    partial void OnSelectedSolutionDirectoryChanged(DirectoryModel? value)
    {
        if (value is null)
            return;

        Task.Run(async () =>
        {
            CurrentGitBranch = await _gitService.GetCurrentBranchName(value.Path);

            var appSettings = AppSettingsService.AppSettings;
            appSettings.CurrentSolutionDirectoryName = value.Name;

            AppSettingsService.SaveAppSettings(appSettings);
        }).ConfigureAwait(false);
    }
    partial void OnSelectedHttpDirectoryChanged(DirectoryModel? value)
    {
        if (value is null)
            return;

        Task.Run(() =>
        {
            var appSettings = AppSettingsService.AppSettings;
            appSettings.CurrentHttpDirectoryName = value.Name;

            AppSettingsService.SaveAppSettings(appSettings);
        }).ConfigureAwait(false);
    }

    #endregion
}