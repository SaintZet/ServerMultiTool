using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    #endregion

    #region Constructor

    public SettingsViewModel(
        IAppSettingsService appSettingsService,
        IPipelineProfilesService pipelineProfilesService,
        GeneralInfoViewModel generalInfo)
    {
        _appSettingsService = appSettingsService;
        _pipelineProfilesService = pipelineProfilesService;
        _generalInfo = generalInfo;

        _isInitializing = true;

        LoadSettings();
        LoadProfiles();
        _pipelineProfilesService.ProfilesChanged += (_, _) => Application.Current.Dispatcher.Invoke(LoadProfiles);

        EditPipelineProfile.PropertyChanged += OnEditPipelineProfilePropertyChanged;

        if (PipelineProfiles.Any())
            SelectedPipelineProfile = PipelineProfiles.First();

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
            SelectedPipelineProfile = PipelineProfiles.FirstOrDefault(p => p.Name == selectedName) ?? PipelineProfiles.FirstOrDefault();
        }
        else if (PipelineProfiles.Any())
        {
            SelectedPipelineProfile = PipelineProfiles.First();
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
        var appSettings = _appSettingsService.Get();

        appSettings.SolutionDirectories = SolutionDirectories.ToStructArray();
        appSettings.HttpDirectories = HttpDirectories.ToStructArray();

        _appSettingsService.Save(appSettings);

        _initialSolutionDirectories = SolutionDirectories.Clone();
        _initialHttpDirectories = HttpDirectories.Clone();

        _pipelineProfilesService.SaveAll(
            [.. PipelineProfiles.Select(w => w.ToOriginal())]
        );

        _initialPipelineProfiles = PipelineProfiles.Clone();

        GeneralInfo.UpdateData();

        HasUnsavedChanges = false;
    }

    [RelayCommand]
    private void CancelSettings()
    {
        _isInitializing = true;

        SolutionDirectories = _initialSolutionDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);
        HttpDirectories = _initialHttpDirectories.CloneWithPropertyChanged(OnDirectoryPropertyChanged);

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

    #endregion
}



