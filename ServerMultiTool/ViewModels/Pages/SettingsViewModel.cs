using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Data;

namespace ServerMultiTool.ViewModels.Pages;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly GeneralInfoViewModel _generalInfo;

    public GeneralInfoViewModel GeneralInfo
    {
        get => _generalInfo;
        init => SetProperty(ref _generalInfo, value);
    }
    
    private bool _hasUnsavedChanges;
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => SetProperty(ref _hasUnsavedChanges, value);
    }

    public ObservableCollection<DirectoryModelWrapper> SolutionDirectories { get; private set; }
    public ObservableCollection<DirectoryModelWrapper> HttpDirectories { get; private set; }
    
    private ObservableCollection<DirectoryModelWrapper> _initialSolutionDirectories;
    private ObservableCollection<DirectoryModelWrapper> _initialHttpDirectories;

    private DirectoryModelWrapper? _selectedSolutionDirectory;
    public DirectoryModelWrapper? SelectedSolutionDirectory
    {
        get => _selectedSolutionDirectory;
        set => SetProperty(ref _selectedSolutionDirectory, value);
    }
    
    private DirectoryModelWrapper? _selectedHttpDirectory;
    public DirectoryModelWrapper? SelectedHttpDirectory
    {
        get => _selectedHttpDirectory;
        set => SetProperty(ref _selectedHttpDirectory, value);
    }
    
    
    public ObservableCollection<PipelineProfile> PipelineProfiles { get; set; } = new();
    private PipelineProfile _selectedPipelineProfile;

    public PipelineProfile SelectedPipelineProfile
    {
        get => _selectedPipelineProfile;
        set => SetProperty(ref _selectedPipelineProfile, value);
    }

    public SettingsViewModel()
    {
        LoadSettings();
        LoadProfiles();
    }

    private void LoadSettings()
    {
        var appSettings = AppSettingsService.AppSettings;

        _initialSolutionDirectories = appSettings.SolutionDirectories.ToWrapperCollection();
        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        
        _initialHttpDirectories = appSettings.HttpDirectories.ToWrapperCollection();
        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
    }

    private void OnDirectoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(DirectoryModelWrapper.Name))
            return;

        HasUnsavedChanges = true;
    }

    private void LoadProfiles()
    {
        var pipelineProfiles = PipelineProfilesService.PipelineProfiles;
        PipelineProfiles = new ObservableCollection<PipelineProfile>(pipelineProfiles);
    }

    [RelayCommand]
    public void SaveSettings()
    {
        HasUnsavedChanges = false;
        
        var appSettings = AppSettingsService.AppSettings;
        
        appSettings.SolutionDirectories = SolutionDirectories.ToStructArray();
        appSettings.HttpDirectories = HttpDirectories.ToStructArray();

        AppSettingsService.SaveAppSettings(appSettings);
        
        _initialSolutionDirectories = SolutionDirectories.Clone();
        _initialHttpDirectories = HttpDirectories.Clone();

        PipelineProfilesService.SavePipelineProfiles(PipelineProfiles.ToList());
        
        GeneralInfo.UpdateData();
    }

    [RelayCommand]
    public void CancelSettings()
    {
        HasUnsavedChanges = false;
        
        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        OnPropertyChanged(nameof(SolutionDirectories));
        
        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        OnPropertyChanged(nameof(HttpDirectories));
    }

    [RelayCommand]
    public void AddPipelineProfile()
    {
        HasUnsavedChanges = true;
        PipelineProfiles.Add(new PipelineProfile
        {
            Name = "New Profile", 
            SettingsPerProject = new ProjectSettings[] { },
        });
    }

    [RelayCommand]
    public void RemovePipelineProfile()
    {
        HasUnsavedChanges = true;
        PipelineProfiles.Remove(SelectedPipelineProfile);
    }

    [RelayCommand]
    public void AddSolutionDirectory()
    {
        HasUnsavedChanges = true;
        var newDirectory = new DirectoryModelWrapper("New Solution", "");
        newDirectory.PropertyChanged += OnDirectoryPropertyChanged;
        SolutionDirectories.Add(newDirectory);
    }

    private bool CanRemoveSolutionDirectory() => 
        SelectedSolutionDirectory != null;

    [RelayCommand(CanExecute = nameof(CanRemoveSolutionDirectory))]
    public void RemoveSolutionDirectory(DirectoryModelWrapper directory)
    {
        HasUnsavedChanges = true;
        
        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        SolutionDirectories.Remove(directory);
    }

    [RelayCommand]
    public void AddHttpDirectory()
    {
        HasUnsavedChanges = true;
        
        var newDirectory = new DirectoryModelWrapper("New HTTP", "");
        newDirectory.PropertyChanged += OnDirectoryPropertyChanged;
        HttpDirectories.Add(newDirectory);
    }

    private bool CanRemoveHttpDirectory() => 
        SelectedHttpDirectory != null;

    [RelayCommand(CanExecute = nameof(CanRemoveHttpDirectory))]
    public void RemoveHttpDirectory(DirectoryModelWrapper directory)
    {
        HasUnsavedChanges = true;
        
        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        HttpDirectories.Remove(directory);
    }

    [RelayCommand]
    public void SelectDirectory(DirectoryModelWrapper directory)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = "Select a folder";
        dialog.ShowNewFolderButton = true;

        if (dialog.ShowDialog() is not DialogResult.OK) 
            return;
        
        directory.Path = dialog.SelectedPath;
        
        HasUnsavedChanges = true;
    }
}
