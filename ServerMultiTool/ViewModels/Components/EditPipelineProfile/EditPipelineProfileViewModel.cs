using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.Features.ContinuousIntegration.Git;
using ServerMultiTool.Model.Features.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ServerMultiTool.ViewModels.Components.EditPipelineProfile;

public partial class EditPipelineProfileViewModel : BaseViewModel
{
    [ObservableProperty] private PipelineProfileWrapper? _profile;
    [ObservableProperty] private PipelineStepWrapper? _selectedStep;
    [ObservableProperty] private ObservableCollection<OperationTypeViewModel> _availableOperationTypes;
    [ObservableProperty] private OperationTypeViewModel? _selectedOperationType;

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
        if (value is null)
            return;

        SelectedStep = value.Steps.FirstOrDefault();
    }

    partial void OnSelectedStepChanged(PipelineStepWrapper? value)
    {

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
            SelectedStep = Profile.Steps.FirstOrDefault();
        }

        OnPropertyChanged();
    }

    [RelayCommand]
    private void AddOperation()
    {
        if (Profile is null || SelectedStep is null || SelectedOperationType is null)
            return;

        Type operationType = SelectedOperationType.OperationType;

        // Create a new instance of the selected operation type
        var operation = Activator.CreateInstance(operationType, SelectedOperationType.Name) as BasePipelineOperation;
        if (operation is null)
            return;

        // Create a wrapper for the operation
        var wrapper = PipelineOperationWrapperFactory.Create(operation);

        // Add the operation to the selected step
        SelectedStep.AddOperation(wrapper);

        OnPropertyChanged();
    }

    [RelayCommand]
    private void RemoveOperation(IPipelineOperationWrapper operation)
    {
        if (Profile is null || SelectedStep is null || operation is null)
            return;

        SelectedStep.RemoveOperation(operation);

        OnPropertyChanged();
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