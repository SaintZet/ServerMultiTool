using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers.SettingsPerProjectWrappers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;

public partial class PipelineProfileWrapper : BaseObservableWrapper
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private GitSettingsWrapper _gitSettings;

    [ObservableProperty]
    private
    InternetInformationSettingsWrapper _internetInformationSettings;

    [ObservableProperty]
    private SqlExecutionSettingsWrapper _sqlExecutionSettings;

    [ObservableProperty]
    private WebBrowserSettingsWrapper _webBrowserSettings;

    [ObservableProperty]
    private LogMonitoringSettingsWrapper _monitorLogFilesSettings;

    [ObservableProperty]
    private HttpMonitoringSettingsWrapper _httpMonitoringSettings;

    [ObservableProperty]
    private ObservableCollection<ProjectSettingsWrapper> _settingsPerProject = [];

    public PipelineProfileWrapper(PipelineProfile profile)
    {
        Name = profile.Name ?? string.Empty;

        GitSettings = new GitSettingsWrapper(profile.GitSettings);
        if (GitSettings != null) GitSettings.PropertyChanged += OnNestedPropertyChanged;

        InternetInformationSettings = new InternetInformationSettingsWrapper(profile.InternetInformationSettings);
        InternetInformationSettings.PropertyChanged += OnNestedPropertyChanged;

        SqlExecutionSettings = new SqlExecutionSettingsWrapper(profile.SqlExecutionSettings);
        SqlExecutionSettings.PropertyChanged += OnNestedPropertyChanged;

        WebBrowserSettings = new WebBrowserSettingsWrapper(profile.WebBrowserSettings);
        WebBrowserSettings.PropertyChanged += OnNestedPropertyChanged;

        MonitorLogFilesSettings = new LogMonitoringSettingsWrapper(profile.MonitorLogFilesSettings);
        MonitorLogFilesSettings.PropertyChanged += OnNestedPropertyChanged;

        HttpMonitoringSettings = new HttpMonitoringSettingsWrapper(profile.HttpMonitoringSettings);
        HttpMonitoringSettings.PropertyChanged += OnNestedPropertyChanged;

        SettingsPerProject = new ObservableCollection<ProjectSettingsWrapper>(
            profile.SettingsPerProject?.Select(s => new ProjectSettingsWrapper(s)) ?? Enumerable.Empty<ProjectSettingsWrapper>());

        foreach (var setting in SettingsPerProject)
        {
            setting.PropertyChanged += OnNestedPropertyChanged;
        }

        SettingsPerProject.CollectionChanged += OnSettingsPerProjectCollectionChanged;
    }

    private void OnNestedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(string.Empty);
    }

    private void OnSettingsPerProjectCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ProjectSettingsWrapper item in e.NewItems)
            {
                item.PropertyChanged += OnNestedPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ProjectSettingsWrapper item in e.OldItems)
            {
                item.PropertyChanged -= OnNestedPropertyChanged;
            }
        }

        OnPropertyChanged(string.Empty);
    }

    public PipelineProfile ToPipelineProfile()
    {
        return new PipelineProfile
        {
            Name = Name,
            SettingsPerProject = Enumerable.Select<ProjectSettingsWrapper, ProjectSettings>(SettingsPerProject, s => s.ToProjectSettings()).ToList(),
            GitSettings = GitSettings.ToGitSettings(),
            InternetInformationSettings = InternetInformationSettings.ToInternetInformationSettings(),
            SqlExecutionSettings = SqlExecutionSettings.ToSqlExecutionSettings(),
            WebBrowserSettings = WebBrowserSettings.ToWebBrowserSettings(),
            MonitorLogFilesSettings = MonitorLogFilesSettings.ToLogMonitoringSettings(),
            HttpMonitoringSettings = HttpMonitoringSettings.ToHttpMonitoringSettings()
        };
    }

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(string.Empty);
    }
}