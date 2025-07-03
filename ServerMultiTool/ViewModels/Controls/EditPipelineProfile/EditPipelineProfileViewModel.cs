using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers.SettingsPerProjectWrappers;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile;

public partial class EditPipelineProfileViewModel : BaseViewModel
{
    private PipelineProfileWrapper? _profile;
    public PipelineProfileWrapper? Profile
    {
        get => _profile;
        private set
        {
            if (SetProperty(ref _profile, value)) 
                UpdateProjectSettings();
        }
    }

    [ObservableProperty] private ObservableCollection<ProjectSettingsWrapper> _projectSettings = new();

    private void UpdateProjectSettings()
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

    public void UpdateFromProfile(PipelineProfileWrapper profile)
    {
        Profile = profile;
    }

    [RelayCommand]
    private void AddProject()
    {
        if (Profile == null) 
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
        if (Profile == null) 
            return;

        Profile.SettingsPerProject.Remove(project);
        ProjectSettings.Remove(project);
        
        OnPropertyChanged(nameof(ProjectSettings));
        OnPropertyChanged(string.Empty);
    }
}