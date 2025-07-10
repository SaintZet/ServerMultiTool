using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers.SettingsPerProjectWrappers;

public partial class ProjectSettingsWrapper : BaseObservableWrapper
{
    private readonly ProjectSettings _settings;

    // Project Properties
    [ObservableProperty] private string _projectName;
    [ObservableProperty] private string _projectPath;

    // MsBuild Settings
    [ObservableProperty] private bool _msBuildEnabled;
    [ObservableProperty] private ObservableCollection<string>? _buildParameters;
    [ObservableProperty] private ObservableCollection<ProcessEventWrapper>? _preBuildEvents;
    [ObservableProperty] private ObservableCollection<ProcessEventWrapper>? _postBuildEvents;

    // Delivery Settings
    [ObservableProperty] private bool _enableCustomDelivery;
    [ObservableProperty] private bool _enableDeliveryBin;
    [ObservableProperty] private ObservableCollection<DeliveryDirectoryWrapper>? _deliveryDirectories;

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
        EnableCustomDelivery = delivery.EnableCustomDelivery;
        EnableDeliveryBin = delivery.EnableDeliveryBin;

        if (delivery.CustomDeliveryDirectories != null)
            DeliveryDirectories = new ObservableCollection<DeliveryDirectoryWrapper>(
                delivery.CustomDeliveryDirectories.Select(d => new DeliveryDirectoryWrapper(d)));
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
            _settings.MsBuildSettings.PreBuildEvents = [..Enumerable.Select<ProcessEventWrapper, ProcessEvent>(PreBuildEvents, e => e.ToProcessEvent())];
        
        if (PostBuildEvents != null)
            _settings.MsBuildSettings.PostBuildEvents = [..Enumerable.Select<ProcessEventWrapper, ProcessEvent>(PostBuildEvents, e => e.ToProcessEvent())];

        _settings.DeliverySettings.EnableCustomDelivery = EnableCustomDelivery;
        _settings.DeliverySettings.EnableDeliveryBin = EnableDeliveryBin;
        
        if (DeliveryDirectories != null)
            _settings.DeliverySettings.CustomDeliveryDirectories = [..DeliveryDirectories.Select(d => d.ToDeliveryDirectory())];

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