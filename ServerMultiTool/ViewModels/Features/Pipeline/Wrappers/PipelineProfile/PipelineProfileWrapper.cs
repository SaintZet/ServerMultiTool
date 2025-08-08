using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Features.Pipeline.Wrappers.Services.GameServerLogs;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;
using System;
using System.Linq;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;

public partial class PipelineProfileWrapper : ObservableObject
{
    [ObservableProperty] string _name = string.Empty;
    [ObservableProperty] string _description = string.Empty;
    [ObservableProperty] public PipelineStepsCollection _steps;
    [ObservableProperty] public GsLogMonitoringSettingsWrapper _gsLogMonitoringSettings;

    readonly PipelineProfile _profile;

    public PipelineProfileWrapper(PipelineProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile), "Pipeline profile cannot be null.");

        Name = profile.Name;
        Description = profile.Description;

        GsLogMonitoringSettings = new(profile.GsLogMonitoringSettings);

        Steps = new PipelineStepCollection(profile.Steps.Select(step => new PipelineStepWrapper(step)));
    }

    public PipelineProfile ToOriginal()
    {
        _profile.UpdateName(Name);
        _profile.UpdateDescription(Description);
        _profile.UpdateGsLogMonitoringSettings(GsLogMonitoringSettings.ToOriginal());

        return _profile;
    }

    public void AddStep(PipelineStep newStep)
    {

    }

    public void RemoveStep(PipelineStep pipelineStep)
    {

    }
}