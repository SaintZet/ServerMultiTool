using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Features.Services.Git;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Components.GeneralInfo;

public partial class GeneralInfoViewModel : BaseViewModel
{
    [ObservableProperty] private bool _canChangeStates = true;

    [ObservableProperty] private string? _currentGitBranch;

    [ObservableProperty] private DirectoryModel[] _solutionDirectories = [];
    [ObservableProperty] private DirectoryModel? _selectedSolutionDirectory;

    [ObservableProperty] private DirectoryModel[] _httpDirectories = [];
    [ObservableProperty] private DirectoryModel? _selectedHttpDirectory;

    private readonly GitService _gitService = new();

    #region Services

    private readonly IAppSettingsService _appSettingsService;

    #endregion

    public GeneralInfoViewModel(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;

        var appSettings = _appSettingsService.Get();

        SelectedSolutionDirectory = appSettings.SolutionDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentSolutionDirectoryName);
        SolutionDirectories = appSettings.SolutionDirectories;

        SelectedHttpDirectory = appSettings.HttpDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentHttpDirectoryName);
        HttpDirectories = appSettings.HttpDirectories;
    }

    public void UpdateData()
    {
        var appSettings = _appSettingsService.Get();

        SolutionDirectories = appSettings.SolutionDirectories;
        HttpDirectories = appSettings.HttpDirectories;
    }

    partial void OnSolutionDirectoriesChanged(DirectoryModel[] value)
    {
        SelectedSolutionDirectory = EnsureSelectedDirectory(SelectedSolutionDirectory, value);
    }

    partial void OnHttpDirectoriesChanged(DirectoryModel[] value)
    {
        SelectedHttpDirectory = EnsureSelectedDirectory(SelectedHttpDirectory, value);
    }

    partial void OnSelectedSolutionDirectoryChanged(DirectoryModel? value)
    {
        UpdateSelectedDirectory(
            selected: value,
            available: SolutionDirectories,
            updateSetting: (ref AppSettings settings, string name) => settings.CurrentSolutionDirectoryName = name
        );

        if (value is null) return;

        Task.Run(async () =>
        {
            CurrentGitBranch = await _gitService.GetCurrentBranchNameAsync(value.Path);
        }).ConfigureAwait(false);

    }

    partial void OnSelectedHttpDirectoryChanged(DirectoryModel? value)
    {
        UpdateSelectedDirectory(
            selected: value,
            available: HttpDirectories,
            updateSetting: (ref AppSettings settings, string name) => settings.CurrentHttpDirectoryName = name
        );
    }

    public delegate void RefAppSettingsUpdate(ref AppSettings settings, string name);

    private void UpdateSelectedDirectory(
    DirectoryModel? selected,
    DirectoryModel[] available,
    RefAppSettingsUpdate updateSetting)
    {
        selected = EnsureSelectedDirectory(selected, available);
        if (selected is null) return;

        Task.Run(() =>
        {
            var appSettings = _appSettingsService.Get();
            updateSetting(ref appSettings, selected.Name);
            _appSettingsService.Save(appSettings);
        }).ConfigureAwait(false);
    }

    private static DirectoryModel? EnsureSelectedDirectory(DirectoryModel? selected, DirectoryModel[] available)
    {
        if (available is null || available.Length is 0)
            return null;

        return (selected is null || available.Contains(selected) is false) ? available.First() : selected;
    }
}