using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile
{
    public partial class EditPipelineProfileViewModel : BaseViewModel
    {
        private PipelineProfile _profile = null!;

        [ObservableProperty] private string _name = null!;

        [ObservableProperty] private bool _gitEnabled;

        [ObservableProperty] private bool _iisEnabled;

        [ObservableProperty] private bool _sqlEnabled;

        [ObservableProperty] private bool _webBrowserEnabled;

        [ObservableProperty] private bool _httpMonitoringEnabled;

        [ObservableProperty] private ObservableCollection<ProjectSettingsWrapper> _projectSettings = null!;

        public void UpdateFromProfile(PipelineProfile profile)
        {
            _profile = profile;
            
            Name = profile.Name;
            GitEnabled = profile.GitSettings.Enable;
            IisEnabled = profile.InternetInformationSettings.Enable;
            SqlEnabled = profile.SqlExecutionSettings.Enable;
            WebBrowserEnabled = profile.WebBrowserSettings.Enable;
            HttpMonitoringEnabled = profile.HttpMonitoringSettings.Enable;
            ProjectSettings = new ObservableCollection<ProjectSettingsWrapper>(
                profile.SettingsPerProject.Select(p => new ProjectSettingsWrapper(p)));
        }

        [RelayCommand]
        private void AddProject() => 
            ProjectSettings.Add(new ProjectSettingsWrapper(new ProjectSettings()));

        [RelayCommand]
        private void RemoveProject(ProjectSettingsWrapper project) => 
            ProjectSettings.Remove(project);

        partial void OnNameChanged(string value) => 
            _profile.Name = value;

        partial void OnGitEnabledChanged(bool value) => 
            _profile.GitSettings.Enable = value;

        partial void OnIisEnabledChanged(bool value) => 
            _profile.InternetInformationSettings.Enable = value;

        partial void OnSqlEnabledChanged(bool value) => 
            _profile.SqlExecutionSettings.Enable = value;

        partial void OnWebBrowserEnabledChanged(bool value) => 
            _profile.WebBrowserSettings.Enable = value;

        partial void OnHttpMonitoringEnabledChanged(bool value) => 
            _profile.HttpMonitoringSettings.Enable = value;
        
        partial void OnProjectSettingsChanging(ObservableCollection<ProjectSettingsWrapper> value) =>
            _profile.SettingsPerProject = [..value.Select(p => p.ToProjectSettings())];
    }
}