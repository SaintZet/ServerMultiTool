using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using ServerMultiTool.Features.Settings;
using ServerMultiTool.Model.Infrastructure.DefaultValues;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Shared.Components.GeneralInfo;
using ServerMultiTool.Shared.Components.ProfileEditor;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Common.Interfaces;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Settings.Extensions;
using ServerMultiTool.ViewModels.Features.Settings.Wrappers;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using Application = System.Windows.Application;
using System.Threading.Tasks;

namespace ServerMultiTool.Features.Settings.Presentation;

public partial class SettingsViewModel : BaseViewModel, IPage, INavigationAware
{
    #region Observable Properties

    public string Title => AppRoutes.Settings.Key;

    [ObservableProperty] private bool _hasUnsavedChanges;

    [ObservableProperty] private string _selectedTabKey = SettingsPageTabKeys.General;

    [ObservableProperty] private DirectoryModelWrapper? _selectedSolutionDirectory;
    [ObservableProperty] private DirectoryModelWrapper? _selectedHttpDirectory;

    [ObservableProperty] private GeneralInfoViewModel _generalInfo;

    // Auto-update settings
    [ObservableProperty] private string _updateFeedUrl = string.Empty;
    [ObservableProperty] private string _updatePublicKey = string.Empty;
    [ObservableProperty] private bool _checkForUpdatesOnStartup;

    partial void OnUpdateFeedUrlChanged(string value)
    {
        if (_isInitializing) return;
        HasUnsavedChanges = true;
    }

    partial void OnCheckForUpdatesOnStartupChanged(bool value)
    {
        if (_isInitializing) return;
        HasUnsavedChanges = true;
    }

    partial void OnUpdatePublicKeyChanged(string value)
    {
        if (_isInitializing) return;
        HasUnsavedChanges = true;
    }

    [ObservableProperty] private PipelineProfileWrapper? _selectedPipelineProfile;
    [ObservableProperty] private EditPipelineProfileViewModel _editPipelineProfile = new();

    partial void OnSelectedPipelineProfileChanged(PipelineProfileWrapper? value)
    {
        if (value is null)
            return;

        EditPipelineProfile.Profile = value;
    }

    #endregion

    #region Private Fields

    private bool _isInitializing;
    private bool _isSaving;

    // Initial state for cancel
    private ObservableCollection<DirectoryModelWrapper> _initialSolutionDirectories = [];
    private ObservableCollection<DirectoryModelWrapper> _initialHttpDirectories = [];
    private PipelineProfilesCollection _initialPipelineProfiles = [];

    #endregion

    #region Collections

    [ObservableProperty] private ObservableCollection<DirectoryModelWrapper> _solutionDirectories = [];
    [ObservableProperty] private ObservableCollection<DirectoryModelWrapper> _httpDirectories = [];
    [ObservableProperty] private PipelineProfilesCollection _pipelineProfiles = [];

    #endregion

    #region Services

    private readonly IAppSettingsService _appSettingsService;
    private readonly IPipelineProfilesService _pipelineProfilesService;
    private readonly IAutoUpdateService _autoUpdateService;

    #endregion

    #region Constructor

    public SettingsViewModel(
        IAppSettingsService appSettingsService,
        IPipelineProfilesService pipelineProfilesService,
        IAutoUpdateService autoUpdateService,
        GeneralInfoViewModel generalInfo)
    {
        _appSettingsService = appSettingsService;
        _pipelineProfilesService = pipelineProfilesService;
        _autoUpdateService = autoUpdateService;
        _generalInfo = generalInfo;

        _isInitializing = true;

        LoadSettings();
        LoadProfiles();
        _pipelineProfilesService.ProfilesChanged += (_, _) => Application.Current.Dispatcher.Invoke(() =>
        {
            if (_isSaving) return;
            LoadProfiles();
        });

        EditPipelineProfile.PropertyChanged += OnEditPipelineProfilePropertyChanged;


        _isInitializing = false;
    }

    #endregion

    #region Load Methods

    private void LoadSettings()
    {
        var appSettings = _appSettingsService.Get();

        _initialSolutionDirectories = appSettings.SolutionDirectories.ToWrapperCollection();
        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);

        _initialHttpDirectories = appSettings.HttpDirectories.ToWrapperCollection();
        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);

        UpdateFeedUrl = appSettings.UpdateFeedUrl ?? string.Empty;
        UpdatePublicKey = appSettings.UpdatePublicKey ?? string.Empty;
        CheckForUpdatesOnStartup = appSettings.CheckForUpdatesOnStartup;
    }

    private void LoadProfiles()
    {
        var selectedName = SelectedPipelineProfile?.Name;

        PipelineProfiles.Clear();

        foreach (var profile in _pipelineProfilesService.GetAll())
        {
            var wrapper = new PipelineProfileWrapper(profile);
            wrapper.PropertyChanged += OnProfilePropertyChanged;
            PipelineProfiles.Add(wrapper);
        }

        _initialPipelineProfiles = PipelineProfiles.Clone();

        RestoreSelectedPipelineProfile(selectedName);
    }

    private void RestoreSelectedPipelineProfile(string? selectedName)
    {
        if (!string.IsNullOrEmpty(selectedName))
        {
            SelectedPipelineProfile = PipelineProfiles.FirstOrDefault(p => p.Name == selectedName);
        }
    }

    #endregion

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is not SettingsNavigationRequest request)
            return;

        if (string.IsNullOrWhiteSpace(request.TabKey) is false)
            SelectedTabKey = request.TabKey;

        if (request.TabKey != SettingsPageTabKeys.PipelineProfiles || string.IsNullOrWhiteSpace(request.PipelineProfileName))
            return;

        SelectedPipelineProfile = PipelineProfiles.FirstOrDefault(profile => profile.Name == request.PipelineProfileName)
                                  ?? SelectedPipelineProfile;
    }

    #region Property Changed Handlers

    private void OnEditPipelineProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isInitializing || e.PropertyName == nameof(EditPipelineProfile.Profile) || e.PropertyName == nameof(EditPipelineProfile.SelectedStep))
            return;

        HasUnsavedChanges = true;
    }

    private void OnDirectoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isInitializing || e.PropertyName is not nameof(DirectoryModelWrapper.Name))
            return;

        HasUnsavedChanges = true;
    }

    private void OnProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        HasUnsavedChanges = true;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void SaveSettings()
    {
        _isSaving = true;
        try
        {
            var appSettings = _appSettingsService.Get();

            appSettings.SolutionDirectories = SolutionDirectories.ToStructArray();
            appSettings.HttpDirectories = HttpDirectories.ToStructArray();
            appSettings.UpdateFeedUrl = UpdateFeedUrl;
            appSettings.UpdatePublicKey = UpdatePublicKey;
            appSettings.CheckForUpdatesOnStartup = CheckForUpdatesOnStartup;

            _appSettingsService.Save(appSettings);

            _initialSolutionDirectories = SolutionDirectories.Clone();
            _initialHttpDirectories = HttpDirectories.Clone();

            _pipelineProfilesService.SaveAll(
                [.. PipelineProfiles.Select(w => w.ToOriginal())]
            );

            _initialPipelineProfiles = PipelineProfiles.Clone();

            // Re-configure the auto-updater with the new settings
            _autoUpdateService.Configure(UpdateFeedUrl, UpdatePublicKey, CheckForUpdatesOnStartup);

            GeneralInfo.UpdateData();

            HasUnsavedChanges = false;
        }
        finally
        {
            _isSaving = false;
        }
    }

    [RelayCommand]
    private void CancelSettings()
    {
        _isInitializing = true;

        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);

        // Reload update settings from last saved state
        var appSettings = _appSettingsService.Get();
        UpdateFeedUrl = appSettings.UpdateFeedUrl ?? string.Empty;
        UpdatePublicKey = appSettings.UpdatePublicKey ?? string.Empty;
        CheckForUpdatesOnStartup = appSettings.CheckForUpdatesOnStartup;

        var selectedName = SelectedPipelineProfile?.Name;

        PipelineProfiles.Clear();

        PipelineProfiles = _initialPipelineProfiles.Clone();

        RestoreSelectedPipelineProfile(selectedName);

        _isInitializing = false;

        HasUnsavedChanges = false;
    }

    [RelayCommand]
    private void AddPipelineProfile()
    {
        var newProfile = DefaultProfiles.GetIisResetProfile();
        newProfile.UpdateName("New Profile");

        var wrapper = new PipelineProfileWrapper(newProfile);
        wrapper.PropertyChanged += OnProfilePropertyChanged;

        PipelineProfiles.Add(wrapper);
        EditPipelineProfile.ExpandPipelineGeneralForNewProfile();
        SelectedPipelineProfile = wrapper;

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void RemovePipelineProfile(PipelineProfileWrapper? profile = null)
    {
        var profileToRemove = profile ?? SelectedPipelineProfile;

        if (profileToRemove is null)
            return;

        profileToRemove.PropertyChanged -= OnProfilePropertyChanged;
        PipelineProfiles.Remove(profileToRemove);

        if (SelectedPipelineProfile == profileToRemove)
            SelectedPipelineProfile = null;

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void AddSolutionDirectory()
    {
        var newDirectory = new DirectoryModelWrapper("New Solution", "");
        newDirectory.PropertyChanged += OnDirectoryPropertyChanged;
        SolutionDirectories.Add(newDirectory);

        HasUnsavedChanges = true;
    }

    private bool CanRemoveSolutionDirectory() =>
        SelectedSolutionDirectory != null;

    [RelayCommand(CanExecute = nameof(CanRemoveSolutionDirectory))]
    private void RemoveSolutionDirectory(DirectoryModelWrapper directory)
    {
        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        SolutionDirectories.Remove(directory);

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void RemoveSolutionDirectoryRow(DirectoryModelWrapper? directory)
    {
        if (directory is null)
            return;

        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        SolutionDirectories.Remove(directory);

        if (SelectedSolutionDirectory == directory)
            SelectedSolutionDirectory = null;

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void AddHttpDirectory()
    {
        var newDirectory = new DirectoryModelWrapper("New HTTP", "");
        newDirectory.PropertyChanged += OnDirectoryPropertyChanged;
        HttpDirectories.Add(newDirectory);

        HasUnsavedChanges = true;
    }

    private bool CanRemoveHttpDirectory() =>
        SelectedHttpDirectory != null;

    [RelayCommand(CanExecute = nameof(CanRemoveHttpDirectory))]
    private void RemoveHttpDirectory(DirectoryModelWrapper directory)
    {
        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        HttpDirectories.Remove(directory);

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void RemoveHttpDirectoryRow(DirectoryModelWrapper? directory)
    {
        if (directory is null)
            return;

        directory.PropertyChanged -= OnDirectoryPropertyChanged;
        HttpDirectories.Remove(directory);

        if (SelectedHttpDirectory == directory)
            SelectedHttpDirectory = null;

        HasUnsavedChanges = true;
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

    [RelayCommand]
    private void RenameDirectory(DirectoryModelWrapper? directory)
    {
        if (directory is null)
            return;

        var currentName = directory.Name;
        var newName = Interaction.InputBox("Enter directory name:", "Rename Directory", currentName);

        if (string.IsNullOrWhiteSpace(newName))
            return;

        var normalizedName = newName.Trim();

        if (normalizedName == currentName)
            return;

        directory.Name = normalizedName;
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        await _autoUpdateService.CheckForUpdatesAsync();
    }

    #endregion
}






