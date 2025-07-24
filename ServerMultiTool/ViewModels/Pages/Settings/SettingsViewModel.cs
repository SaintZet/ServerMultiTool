using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Common.DefaultValues;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Controls.EditPipelineProfile;
using ServerMultiTool.ViewModels.Pages.Settings.Extensions;
using ServerMultiTool.ViewModels.Pages.Settings.Wrappers;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Application = System.Windows.Application;

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

    private bool _isInitializing;
    private bool _isChangingProfile;

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

    public ObservableCollection<PipelineProfileWrapper> PipelineProfiles { get; private set; } = [];
    private ObservableCollection<PipelineProfileWrapper> _initialPipelineProfiles = [];

    private PipelineProfileWrapper? _selectedPipelineProfile;
    public PipelineProfileWrapper? SelectedPipelineProfile
    {
        get => _selectedPipelineProfile;
        set
        {
            if (value is null || !SetProperty(ref _selectedPipelineProfile, value))
                return;

            _isChangingProfile = true;
            EditPipelineProfile.UpdateFromProfile(value);
            _isChangingProfile = false;
        }
    }

    private string _selectedTabKey = SetiignsPageTabKeys.General;
    public string SelectedTabKey
    {
        get => _selectedTabKey;
        set => SetProperty(ref _selectedTabKey, value);
    }

    public SettingsViewModel()
    {
        _isInitializing = true;

        LoadSettings();
        LoadProfiles();
        PipelineProfilesService.PipelineProfilesChanged += (_, _) => Application.Current.Dispatcher.Invoke(LoadProfiles);

        EditPipelineProfile.PropertyChanged += OnEditPipelineProfilePropertyChanged;

        if (PipelineProfiles.Any())
            SelectedPipelineProfile = PipelineProfiles.First();

        _isInitializing = false;
    }

    private void LoadSettings()
    {
        var appSettings = AppSettingsService.AppSettings;

        _initialSolutionDirectories = appSettings.SolutionDirectories.ToWrapperCollection();
        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);

        _initialHttpDirectories = appSettings.HttpDirectories.ToWrapperCollection();
        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
    }

    private void LoadProfiles()
    {
        var selectedName = SelectedPipelineProfile?.Name;

        PipelineProfiles.Clear();

        foreach (var profile in PipelineProfilesService.PipelineProfiles)
        {
            var wrapper = new PipelineProfileWrapper(profile);
            wrapper.PropertyChanged += OnProfilePropertyChanged;
            PipelineProfiles.Add(wrapper);
        }

        OnPropertyChanged(nameof(PipelineProfiles));

        if (string.IsNullOrEmpty(selectedName) is false)
        {
            SelectedPipelineProfile = PipelineProfiles.FirstOrDefault(p => p.Name == selectedName) ?? PipelineProfiles.FirstOrDefault();
        }
        else if (PipelineProfiles.Any())
        {
            SelectedPipelineProfile = PipelineProfiles.First();
        }
    }

    private void OnEditPipelineProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isInitializing || _isChangingProfile)
            return;

        HasUnsavedChanges = true;
    }

    private void OnDirectoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isInitializing || e.PropertyName is not nameof(DirectoryModelWrapper.Name))
            return;

        HasUnsavedChanges = true;
    }

    private void OnPipelineProfilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        if (e.NewItems != null)
            foreach (PipelineProfileWrapper wrapper in e.NewItems)
                wrapper.PropertyChanged += OnProfilePropertyChanged;

        if (e.OldItems != null)
            foreach (PipelineProfileWrapper wrapper in e.OldItems)
                wrapper.PropertyChanged -= OnProfilePropertyChanged;

        HasUnsavedChanges = true;
    }

    private void OnProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        HasUnsavedChanges = true;
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

        PipelineProfilesService.SavePipelineProfiles(
            PipelineProfiles.Select(w => w.ToPipelineProfile()).ToList()
        );

        _initialPipelineProfiles = new ObservableCollection<PipelineProfileWrapper>(
            PipelineProfiles.Select(w => new PipelineProfileWrapper(w.ToPipelineProfile()))
        );

        GeneralInfo.UpdateData();
    }

    [RelayCommand]
    private void CancelSettings()
    {
        _isInitializing = true;
        HasUnsavedChanges = false;

        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        OnPropertyChanged(nameof(SolutionDirectories));

        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        OnPropertyChanged(nameof(HttpDirectories));

        PipelineProfiles.Clear();
        foreach (var wrapper in _initialPipelineProfiles)
        {
            var newWrapper = new PipelineProfileWrapper(wrapper.ToPipelineProfile());
            newWrapper.PropertyChanged += OnProfilePropertyChanged;
            PipelineProfiles.Add(newWrapper);
        }

        SelectedPipelineProfile = PipelineProfiles.Any() ? PipelineProfiles.First() : null;
        OnPropertyChanged(nameof(PipelineProfiles));

        _isInitializing = false;
    }

    [RelayCommand]
    private void AddPipelineProfile()
    {
        // todo: maybe change default profile
        var newProfile = DefaultProfiles.GetIisResetProfile();
        newProfile.Name = "New Profile";

        var wrapper = new PipelineProfileWrapper(newProfile);
        wrapper.PropertyChanged += OnProfilePropertyChanged;

        PipelineProfiles.Add(wrapper);
        SelectedPipelineProfile = wrapper;

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void RemovePipelineProfile()
    {
        if (SelectedPipelineProfile is null)
            return;

        SelectedPipelineProfile.PropertyChanged -= OnProfilePropertyChanged;
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