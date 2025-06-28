using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
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
    private ObservableCollection<string> _buildParameters;

    [ObservableProperty]
    private ObservableCollection<ProcessEventWrapper> _preBuildEvents;

    [ObservableProperty]
    private ObservableCollection<ProcessEventWrapper> _postBuildEvents;

    // Delivery Settings
    [ObservableProperty]
    private bool _deliveryEnabled;

    [ObservableProperty]
    private bool _deliveryBin;

    [ObservableProperty]
    private ObservableCollection<DeliveryDirectoryWrapper> _deliveryDirectories;

    public ProjectSettingsWrapper(ProjectSettings settings)
    {
        _settings = settings;
        
        // Project Properties
        ProjectName = settings.Project.Name;
        ProjectPath = settings.Project.Path;
        
        // MsBuild Settings
        var msBuild = settings.MsBuildSettings;
        MsBuildEnabled = msBuild.Enable;
        BuildParameters = new ObservableCollection<string>(msBuild.Parameters);

        if (msBuild.PreBuildEvents is not null)
            PreBuildEvents = new ObservableCollection<ProcessEventWrapper>(
                msBuild.PreBuildEvents.Select(e => new ProcessEventWrapper(e)));

        if (msBuild.PostBuildEvents is not null)
            PostBuildEvents = new ObservableCollection<ProcessEventWrapper>(
                msBuild.PostBuildEvents.Select(e => new ProcessEventWrapper(e)));
        
        // Delivery Settings
        var delivery = settings.DeliverySettings;
        DeliveryEnabled = delivery.Enable;
        DeliveryBin = delivery.DeliveryBin;
        
        if (delivery.DeliveryDirectory is not null)
            DeliveryDirectories = new ObservableCollection<DeliveryDirectoryWrapper>(
                delivery.DeliveryDirectory.Select(d => new DeliveryDirectoryWrapper(d)));
    }

    public ProjectSettings ToProjectSettings()
    {
        // Update Project
        var project = new DirectoryModel
        {
            Name = ProjectName,
            Path = ProjectPath
        };
        _settings.Project = project;

        // Update MsBuild Settings
        if (_settings.MsBuildSettings == null)
            _settings.MsBuildSettings = new MsBuildSettings();
            
        _settings.MsBuildSettings.Enable = MsBuildEnabled;
        _settings.MsBuildSettings.Parameters = BuildParameters?.ToArray();
        _settings.MsBuildSettings.PreBuildEvents = PreBuildEvents?.Select<ProcessEventWrapper, ProcessEvent>(e => e.ToProcessEvent()).ToArray();
        _settings.MsBuildSettings.PostBuildEvents = PostBuildEvents?.Select<ProcessEventWrapper, ProcessEvent>(e => e.ToProcessEvent()).ToArray();

        // Update Delivery Settings
        if (_settings.DeliverySettings == null)
            _settings.DeliverySettings = new DeliverySettings();
            
        _settings.DeliverySettings.Enable = DeliveryEnabled;
        _settings.DeliverySettings.DeliveryBin = DeliveryBin;
        _settings.DeliverySettings.DeliveryDirectory = DeliveryDirectories?
            .Select(d => d?.ToDeliveryDirectory())
            .ToArray<DeliveryDirectories>();

        return _settings;
    }
}

public partial class ProcessEventWrapper : ObservableObject
{
    [ObservableProperty]
    private string _path;

    [ObservableProperty]
    private string _arguments;

    public ProcessEventWrapper(ProcessEvent processEvent)
    {
        Path = processEvent.Path;
        Arguments = processEvent.Arguments;
    }

    public ProcessEvent ToProcessEvent() => new()
    {
        Path = Path,
        Arguments = Arguments
    };
}

public partial class DeliveryDirectoryWrapper : ObservableObject
{
    [ObservableProperty]
    private string _source;

    [ObservableProperty]
    private string _destination;

    public DeliveryDirectoryWrapper(DeliveryDirectories directory)
    {
        Source = directory.Source;
        Destination = directory.Destination;
    }

    public DeliveryDirectories ToDeliveryDirectory() => new()
    {
        Source = Source,
        Destination = Destination
    };
}