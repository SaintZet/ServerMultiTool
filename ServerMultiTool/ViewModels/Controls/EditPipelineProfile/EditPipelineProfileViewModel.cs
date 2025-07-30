using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers.SettingsPerProjectWrappers;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile;

public partial class EditPipelineProfileViewModel : BaseViewModel
{
    [ObservableProperty] private ObservableCollection<ProjectSettingsWrapper> _projectSettings = [];

    [ObservableProperty] private PipelineProfileWrapper? _profile;

    partial void OnProfileChanged(PipelineProfileWrapper? value)
    {
        ProjectSettings.Clear();

        if (Profile?.SettingsPerProject is null)
            return;

        foreach (var setting in Profile.SettingsPerProject)
        {
            setting.PropertyChanged += OnProjectSettingPropertyChanged;
            ProjectSettings.Add(setting);
        }
    }

    private void OnProjectSettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ProjectSettings));
        OnPropertyChanged(string.Empty);
    }

    [RelayCommand]
    private void AddProject()
    {
        if (Profile is null)
            return;

        var newProjectSettings = new ProjectSettings();
        var newProjectSettingsWrapper = new ProjectSettingsWrapper(newProjectSettings);
        Profile.SettingsPerProject.Add(newProjectSettingsWrapper);
        ProjectSettings.Add(newProjectSettingsWrapper);

        OnPropertyChanged(nameof(ProjectSettings));
        OnPropertyChanged(string.Empty);
    }

    [RelayCommand]
    private void RemoveProject(ProjectSettingsWrapper project)
    {
        if (Profile is null)
            return;

        Profile.SettingsPerProject.Remove(project);
        ProjectSettings.Remove(project);

        if (ProjectSettings.Count is 0)
            AddProject();

        OnPropertyChanged(nameof(ProjectSettings));
        OnPropertyChanged(string.Empty);
    }
}