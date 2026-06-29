using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.Pipeline.Step;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;
using PipelineProfileModel = ServerMultiTool.Model.Features.Pipeline.Profile.PipelineProfile;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineProfile;

public partial class PipelineProfileWrapper : ObservableObject
{
    [ObservableProperty] string _name = string.Empty;
    [ObservableProperty] string _description = string.Empty;
    [ObservableProperty] public PipelineStepsCollection _steps;

    readonly PipelineProfileModel _profile;

    public PipelineProfileWrapper(PipelineProfileModel profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile), "Pipeline profile cannot be null.");

        Name = profile.Name;
        Description = profile.Description;

        Steps = new PipelineStepCollection(profile.Steps.Select(step => new PipelineStepWrapper(step)));
    }

    public PipelineProfileModel ToOriginal()
    {
        _profile.UpdateName(Name);
        _profile.UpdateDescription(Description);
        _profile.UpdateSteps([.. Steps.Select(step => step.ToOriginal())]);

        return _profile;
    }

    public void AddStep(PipelineStep newStep)
    {

    }

    public void RemoveStep(PipelineStep pipelineStep)
    {

    }
}
