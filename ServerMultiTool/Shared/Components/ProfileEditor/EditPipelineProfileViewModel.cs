using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.Model.Features.Pipeline.Operations.FileDelivery;
using ServerMultiTool.Model.Features.Pipeline.Operations.Network;
using ServerMultiTool.Model.Features.Pipeline.Step;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineOperations.Execution;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineOperations.FileDelivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineProfile;

namespace ServerMultiTool.Shared.Components.ProfileEditor;

public partial class EditPipelineProfileViewModel : BaseViewModel, IEditPipelineProfileViewModel
{
    [ObservableProperty] private PipelineProfileWrapper? _profile;
    [ObservableProperty] private PipelineStepWrapper? _selectedStep;
    [ObservableProperty] private PipelineOperationWrapper? _selectedOperation;
    [ObservableProperty] private ObservableCollection<OperationTypeViewModel> _availableOperationTypes;
    [ObservableProperty] private OperationTypeViewModel? _selectedOperationType;
    [ObservableProperty] private bool _isPipelineGeneralExpanded;
    [ObservableProperty] private bool _isStepGeneralExpanded;
    [ObservableProperty] private bool _isOperationGeneralExpanded;
    private int _selectedStepIndex = -1;
    private bool _expandPipelineGeneralOnNextProfileChange;
    private bool _expandStepGeneralOnNextSelection;
    private bool _expandOperationGeneralOnNextSelection;

    public EditPipelineProfileViewModel()
    {
        _availableOperationTypes =
        [
            new OperationTypeViewModel("Git Pull", typeof(GitPullOperation)),
            new OperationTypeViewModel("MS Build", typeof(MsBuildOperation)),
            new OperationTypeViewModel("Delivery Bin", typeof(DeliveryBinOperation)),
            new OperationTypeViewModel("Delivery Files", typeof(DeliverySpecifiedFilesOperation)),
            new OperationTypeViewModel("HTTP Ping", typeof(HttpPingOperation)),
            new OperationTypeViewModel("Process Execution", typeof(ProcessExecutionOperation)),
            new OperationTypeViewModel("SQL Execution", typeof(SqlExecutionOperation)),
            new OperationTypeViewModel("Browser", typeof(WebBrowserOperation))
        ];

        _selectedOperationType = _availableOperationTypes.FirstOrDefault();
    }

    partial void OnProfileChanged(PipelineProfileWrapper? value)
    {
        IsPipelineGeneralExpanded = _expandPipelineGeneralOnNextProfileChange;
        _expandPipelineGeneralOnNextProfileChange = false;

        SelectedStep = null;
        SelectedOperation = null;
    }

    partial void OnSelectedStepChanged(PipelineStepWrapper? value)
    {
        IsStepGeneralExpanded = _expandStepGeneralOnNextSelection;
        _expandStepGeneralOnNextSelection = false;

        _selectedStepIndex = value is null || Profile is null
            ? -1
            : Profile.Steps.IndexOf(value);

        SelectedOperation = null;
    }

    partial void OnSelectedOperationChanged(PipelineOperationWrapper? value)
    {
        IsOperationGeneralExpanded = value is not null && _expandOperationGeneralOnNextSelection;
        _expandOperationGeneralOnNextSelection = false;
    }

    partial void OnSelectedStepChanging(PipelineStepWrapper? value)
    {
        if (SelectedStep is not null)
        {
            SelectedStep.PropertyChanged -= SelectedStep_PropertyChanged;
            SelectedStep.OperationFieldChanged -= SelectedStep_OperationFieldChanged;
        }

        if (value is not null)
        {
            value.PropertyChanged += SelectedStep_PropertyChanged;
            value.OperationFieldChanged += SelectedStep_OperationFieldChanged;
        }
    }

    private void SelectedStep_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged();
    }

    private void SelectedStep_OperationFieldChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged();
    }

    [RelayCommand]
    private void AddStep()
    {
        if (Profile is null)
            return;

        var newStep = new PipelineStep($"Step {Profile.Steps.Count + 1}", $"Description for Step {Profile.Steps.Count + 1}", Profile.Steps.Count);
        var newStepWrapper = new PipelineStepWrapper(newStep);
        Profile.AddStep(newStep);
        Profile.Steps.Add(newStepWrapper);
        _expandStepGeneralOnNextSelection = true;
        SelectedStep = newStepWrapper;

        OnPropertyChanged();
    }

    [RelayCommand]
    private void RemoveStep(PipelineStepWrapper step)
    {
        if (Profile is null || step is null)
            return;

        Profile.RemoveStep(step.ToOriginal());
        Profile.Steps.Remove(step);

        if (SelectedStep == step)
        {
            SelectedStep = null;
        }

        OnPropertyChanged();
    }

    [RelayCommand]
    private void AddOperation()
    {
        if (Profile is null || SelectedStep is null || SelectedOperationType is null)
            return;

        Type operationType = SelectedOperationType.OperationType;

        // todo: delete after develop
        try
        {
            var operation = Activator.CreateInstance(operationType, SelectedOperationType.Name) as PipelineOperationBase;
            if (operation is null)
                return;

            // Create a wrapper for the operation
            var wrapper = PipelineOperationWrapperFactory.Create(operation);

            // Add the operation to the selected step
            SelectedStep.AddOperation(wrapper);
            _expandOperationGeneralOnNextSelection = true;
            SelectedOperation = wrapper;

            OnPropertyChanged();
        }
        catch (Exception ex)
        {
            return;
        }
    }

    [RelayCommand]
    private void SelectOperation(PipelineOperationWrapper? operation)
    {
        SelectedOperation = operation;
    }

    [RelayCommand]
    private void RemoveOperation(PipelineOperationWrapper operation)
    {
        if (Profile is null || SelectedStep is null || operation is null)
            return;

        SelectedStep.RemoveOperation(operation);

        if (SelectedOperation == operation)
            SelectedOperation = null;

        OnPropertyChanged();
    }

    [RelayCommand]
    private void BrowseOperationFileName()
    {
        if (SelectedOperation is not ProcessExecutionOperationWrapper processExec)
            return;
        using var dialog = new OpenFileDialog { Title = "Select executable file", Filter = "All Files|*.*|Executables|*.exe" };
        if (dialog.ShowDialog() == DialogResult.OK)
            processExec.FileName = dialog.FileName;
    }

    [RelayCommand]
    private void BrowseDeliverySource(DeliveryDirectoryWrapper dir)
    {
        if (dir is null) return;
        using var dialog = new FolderBrowserDialog { Description = "Select source directory", ShowNewFolderButton = true };
        if (dialog.ShowDialog() == DialogResult.OK)
            dir.Source = dialog.SelectedPath;
    }

    [RelayCommand]
    private void BrowseDeliveryDestination(DeliveryDirectoryWrapper dir)
    {
        if (dir is null) return;
        using var dialog = new FolderBrowserDialog { Description = "Select destination directory", ShowNewFolderButton = true };
        if (dialog.ShowDialog() == DialogResult.OK)
            dir.Destination = dialog.SelectedPath;
    }

    [RelayCommand]
    private void ToggleOperationEnabled(PipelineOperationWrapper operation)
    {
        if (Profile is null || SelectedStep is null || operation is null)
            return;

        operation.Enabled = !operation.Enabled;

        OnPropertyChanged();
    }

    public void ReorderSteps(PipelineStepWrapper draggedItem, PipelineStepWrapper targetItem)
    {
        if (Profile == null || draggedItem == null || targetItem == null)
            return;

        var draggedIndex = Profile.Steps.IndexOf(draggedItem);
        var targetIndex = Profile.Steps.IndexOf(targetItem);

        if (draggedIndex < 0 || targetIndex < 0)
            return;

        Profile.Steps.ReorderItem(draggedItem, targetItem);

        SelectedStep = draggedItem;

        OnPropertyChanged();
    }

    public void ExpandPipelineGeneralForNewProfile()
    {
        _expandPipelineGeneralOnNextProfileChange = true;
    }
}

public class OperationTypeViewModel
{
    public string Name { get; }
    public Type OperationType { get; }

    public OperationTypeViewModel(string name, Type operationType)
    {
        Name = name;
        OperationType = operationType;
    }
}

