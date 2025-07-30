using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Controls;

public partial class GeneralInfoViewModel : BaseViewModel
{
    [ObservableProperty] private bool _canChangeStates = true;

    [ObservableProperty] private string? _currentGitBranch;

    [ObservableProperty] private DirectoryModel[] _solutionDirectories = [];
    [ObservableProperty] private DirectoryModel? _selectedSolutionDirectory;

    [ObservableProperty] private DirectoryModel[] _httpDirectories = [];
    [ObservableProperty] private DirectoryModel? _selectedHttpDirectory;

    private readonly GitService _gitService = new();

    public GeneralInfoViewModel()
    {
        var appSettings = AppSettingsService.AppSettings;

        SelectedSolutionDirectory = appSettings.SolutionDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentSolutionDirectoryName);
        SolutionDirectories = appSettings.SolutionDirectories;

        SelectedHttpDirectory = appSettings.HttpDirectories.FirstOrDefault(x => x.Name == appSettings.CurrentHttpDirectoryName);
        HttpDirectories = appSettings.HttpDirectories;
    }

    public void UpdateData()
    {
        var appSettings = AppSettingsService.LoadOrInitialize();

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
            CurrentGitBranch = await _gitService.GetCurrentBranchName(value.Path);
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

    private static void UpdateSelectedDirectory(
    DirectoryModel? selected,
    DirectoryModel[] available,
    RefAppSettingsUpdate updateSetting)
    {
        selected = EnsureSelectedDirectory(selected, available);
        if (selected is null) return;

        Task.Run(() =>
        {
            var appSettings = AppSettingsService.AppSettings;
            updateSetting(ref appSettings, selected.Name);
            AppSettingsService.SaveAppSettings(appSettings);
        }).ConfigureAwait(false);
    }

    private static DirectoryModel? EnsureSelectedDirectory(DirectoryModel? selected, DirectoryModel[] available)
    {
        if (available is null || available.Length is 0)
            return null;

        return (selected is null || available.Contains(selected) is false) ? available.First() : selected;
    }
}