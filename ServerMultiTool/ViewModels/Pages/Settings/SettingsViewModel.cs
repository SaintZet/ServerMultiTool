using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common.DefaultValues;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.Settings.Extensions;
using ServerMultiTool.ViewModels.Pages.Settings.Wrappers;
using EditPipelineProfileViewModel = ServerMultiTool.ViewModels.Controls.EditPipelineProfile.EditPipelineProfileViewModel;

namespace ServerMultiTool.ViewModels.Pages.Settings;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly GeneralInfoViewModel _generalInfo;
    public GeneralInfoViewModel GeneralInfo
    {
        get => _generalInfo;
        init => SetProperty(ref _generalInfo, value);
    }

    private readonly EditPipelineProfileViewModel _editPipelineProfile = new();
    public EditPipelineProfileViewModel EditPipelineProfile
    {
        get => _editPipelineProfile;
        init => SetProperty(ref _editPipelineProfile, value);
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
    
    private PipelineProfile? _selectedPipelineProfile;
    public PipelineProfile? SelectedPipelineProfile
    {
        get => _selectedPipelineProfile;
        set
        {
            if (value is null|| !SetProperty(ref _selectedPipelineProfile, value)) 
               return;

            EditPipelineProfile.UpdateFromProfile(value);
        }
    }

    public SettingsViewModel()
    {
        LoadSettings();
        LoadProfiles();

        if (PipelineProfiles.Any()) 
            SelectedPipelineProfile = PipelineProfiles.First();
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
    private void SaveSettings()
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
    private void CancelSettings()
    {
        HasUnsavedChanges = false;
        
        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        OnPropertyChanged(nameof(SolutionDirectories));
        
        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        OnPropertyChanged(nameof(HttpDirectories));
    }

    [RelayCommand]
    private void AddPipelineProfile()
    {
        var newProfile = DefaultProfiles.GetDevProfile();
        newProfile.Name = "New Profile";
        
        PipelineProfiles.Add(newProfile);
        SelectedPipelineProfile = newProfile;
        
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void RemovePipelineProfile()
    {
        if (SelectedPipelineProfile is null) 
            return;
        
        PipelineProfiles.Remove(SelectedPipelineProfile);
        SelectedPipelineProfile = PipelineProfiles.Any() ? PipelineProfiles.First() : null;

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void AddSolutionDirectory()
    {
        HasUnsavedChanges = true;
        var newDirectory = new DirectoryModelWrapper("New Solution", "");
        newDirectory.PropertyChanged += OnDirectoryPropertyChanged;
        SolutionDirectories.Add(newDirectory);
    }

    private bool CanRemoveSolutionDirectory() => 
        SelectedSolutionDirectory != null;

    [RelayCommand(CanExecute = nameof(CanRemoveSolutionDirectory))]
    private void RemoveSolutionDirectory(DirectoryModelWrapper directory)
    {
        HasUnsavedChanges = true;
        
        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        SolutionDirectories.Remove(directory);
    }

    [RelayCommand]
    private void AddHttpDirectory()
    {
        HasUnsavedChanges = true;
        
        var newDirectory = new DirectoryModelWrapper("New HTTP", "");
        newDirectory.PropertyChanged += OnDirectoryPropertyChanged;
        HttpDirectories.Add(newDirectory);
    }

    private bool CanRemoveHttpDirectory() => 
        SelectedHttpDirectory != null;

    [RelayCommand(CanExecute = nameof(CanRemoveHttpDirectory))]
    private void RemoveHttpDirectory(DirectoryModelWrapper directory)
    {
        HasUnsavedChanges = true;
        
        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        HttpDirectories.Remove(directory);
    }

    [RelayCommand]
    private void SelectDirectory(DirectoryModelWrapper directory)
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