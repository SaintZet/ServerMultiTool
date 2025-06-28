using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile;

public partial class EditPipelineProfileViewModel : BaseViewModel
{
    private PipelineProfile? _profile;

    public PipelineProfile? Profile
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
            ProjectSettings.Add(new ProjectSettingsWrapper(setting));
    }

    public void UpdateFromProfile(PipelineProfile profile)
    {
        Profile = profile;
    }

    [RelayCommand]
    private void AddProject()
    {
        if (Profile == null) 
            return;

        var newProjectSetting = new ProjectSettings();
        Profile.SettingsPerProject.Add(newProjectSetting);
        ProjectSettings.Add(new ProjectSettingsWrapper(newProjectSetting));
    }

    [RelayCommand]
    private void RemoveProject(ProjectSettingsWrapper project)
    {
        if (Profile == null) 
            return;

        var originalProject = project.ToProjectSettings();
        Profile.SettingsPerProject.Remove(originalProject);
        ProjectSettings.Remove(project);
    }
}