using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile
{
    public partial class EditPipelineProfileViewModel : ObservableObject
    {
        private PipelineProfile _profile;

        [ObservableProperty] private string _name;

        [ObservableProperty] private bool _gitEnabled;

        [ObservableProperty] private bool _iisEnabled;

        [ObservableProperty] private bool _sqlEnabled;

        [ObservableProperty] private bool _webBrowserEnabled;

        [ObservableProperty] private bool _httpMonitoringEnabled;

        [ObservableProperty] private ObservableCollection<ProjectSettingsWrapper> _projectSettings;

        public void UpdateFromProfile(PipelineProfile profile)
        {
            _profile = profile;
            
            Name = profile.Name;
            GitEnabled = profile.GitSettings?.Enable ?? false;
            IisEnabled = profile.InternetInformationSettings?.Enable  ?? false;
            SqlEnabled = profile.SqlExecutionSettings?.Enable  ?? false;
            WebBrowserEnabled = profile.WebBrowserSettings?.Enable  ?? false;
            HttpMonitoringEnabled = profile.HttpMonitoringSettings?.Enable  ?? false;
            ProjectSettings = new ObservableCollection<ProjectSettingsWrapper>(
                profile.SettingsPerProject.Select(p => new ProjectSettingsWrapper(p)));
        }

        [RelayCommand]
        private void AddProject()
        {
            ProjectSettings.Add(new ProjectSettingsWrapper(new ProjectSettings()));
        }

        [RelayCommand]
        private void RemoveProject(ProjectSettingsWrapper project)
        {
            ProjectSettings.Remove(project);
        }

        [RelayCommand]
        public void AddBuildParameter()
        {
            
        }

        [RelayCommand]
        public void AddPreBuildEvent()
        {
            
        }

        [RelayCommand]
        public void AddPostBuildEvent()
        {
            
        }

        [RelayCommand]
        public void AddDeliveryDirectory()
        {
            
        }

        partial void OnNameChanged(string value) => _profile.Name = value;

        partial void OnGitEnabledChanged(bool value) => _profile.GitSettings.Enable = value;

        partial void OnIisEnabledChanged(bool value) => _profile.InternetInformationSettings.Enable = value;

        partial void OnSqlEnabledChanged(bool value) => _profile.SqlExecutionSettings.Enable = value;

        partial void OnWebBrowserEnabledChanged(bool value) => _profile.WebBrowserSettings.Enable = value;

        partial void OnHttpMonitoringEnabledChanged(bool value) => _profile.HttpMonitoringSettings.Enable = value;
        
        partial void OnProjectSettingsChanging(ObservableCollection<Wrappers.ProjectSettingsWrapper> value) =>
            _profile.SettingsPerProject = value.Select(p => p.ToProjectSettings()).ToArray();
    }
}