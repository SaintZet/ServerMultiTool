using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

public partial class ProjectSettingsWrapper : ObservableObject
{
    private readonly ProjectSettings _settings;

    // Project Properties
    [ObservableProperty]
    private string _projectName;

    [ObservableProperty]
    private string _projectPath;

    // MsBuild Settings
    [ObservableProperty]
    private bool _msBuildEnabled;

    [ObservableProperty]
    private ObservableCollection<string>? _buildParameters;

    [ObservableProperty]
    private ObservableCollection<ProcessEventWrapper>? _preBuildEvents;

    [ObservableProperty]
    private ObservableCollection<ProcessEventWrapper>? _postBuildEvents;

    // Delivery Settings
    [ObservableProperty]
    private bool _deliveryEnabled;

    [ObservableProperty]
    private bool _deliveryBin;

    [ObservableProperty]
    private ObservableCollection<DeliveryDirectoryWrapper>? _deliveryDirectories;

    public ProjectSettingsWrapper(ProjectSettings settings)
    {
        _settings = settings;
        
        // Project Properties
        ProjectName = settings.Project.Name;
        ProjectPath = settings.Project.Path;
        
        // MsBuild Settings
        var msBuild = settings.MsBuildSettings;
        MsBuildEnabled = msBuild.Enable;

        if (msBuild.Parameters != null) 
            BuildParameters = new ObservableCollection<string>(msBuild.Parameters);

        if (msBuild.PreBuildEvents != null)
            PreBuildEvents = new ObservableCollection<ProcessEventWrapper>(
                msBuild.PreBuildEvents.Select(e => new ProcessEventWrapper(e)));

        if (msBuild.PostBuildEvents != null)
            PostBuildEvents = new ObservableCollection<ProcessEventWrapper>(
                msBuild.PostBuildEvents.Select(e => new ProcessEventWrapper(e)));

        // Delivery Settings
        var delivery = settings.DeliverySettings;
        DeliveryEnabled = delivery.Enable;
        DeliveryBin = delivery.DeliveryBin;

        if (delivery.DeliveryDirectory != null)
            DeliveryDirectories = new ObservableCollection<DeliveryDirectoryWrapper>(
                delivery.DeliveryDirectory.Select(d => new DeliveryDirectoryWrapper(d)));
    }

    public ProjectSettings ToProjectSettings()
    {
        var project = new DirectoryModel
        {
            Name = ProjectName,
            Path = ProjectPath
        };
        
        _settings.Project = project;

        _settings.MsBuildSettings.Enable = MsBuildEnabled;
        _settings.MsBuildSettings.Parameters = BuildParameters?.ToList();
        
        if (PreBuildEvents != null)
            _settings.MsBuildSettings.PreBuildEvents = [..PreBuildEvents.Select<ProcessEventWrapper, ProcessEvent>(e => e.ToProcessEvent())];
        
        if (PostBuildEvents != null)
            _settings.MsBuildSettings.PostBuildEvents = [..PostBuildEvents.Select<ProcessEventWrapper, ProcessEvent>(e => e.ToProcessEvent())];

        _settings.DeliverySettings.Enable = DeliveryEnabled;
        _settings.DeliverySettings.DeliveryBin = DeliveryBin;
        
        if (DeliveryDirectories != null)
            _settings.DeliverySettings.DeliveryDirectory = [..DeliveryDirectories.Select(d => d.ToDeliveryDirectory())];

        return _settings;
    }
    
    [RelayCommand]
    private void AddBuildParameter(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return;
        
        BuildParameters?.Add(parameter);
    }
    
    [RelayCommand]
    private void AddPreBuildEvent(ProcessEventWrapper? processEvent)
    {
        if (processEvent == null)
            return;
        
        PreBuildEvents ??= [];
        PreBuildEvents.Add(processEvent);
    }
    
    [RelayCommand]
    private void AddPostBuildEvent(ProcessEventWrapper? processEvent)
    {
        if (processEvent == null)
            return;
        
        PostBuildEvents ??= [];
        PostBuildEvents.Add(processEvent);
    }

    [RelayCommand]
    private void AddDeliveryDirectory(DeliveryDirectoryWrapper? deliveryDirectory)
    {
        if (deliveryDirectory == null)
            return;
        
        DeliveryDirectories ??= [];
        DeliveryDirectories.Add(deliveryDirectory);
    }
}