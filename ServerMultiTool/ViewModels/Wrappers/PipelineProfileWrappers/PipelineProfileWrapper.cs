using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;
using System;
using System.Linq;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;

public partial class PipelineProfileWrapper : ObservableObject
{
    [ObservableProperty] string _name = string.Empty;
    [ObservableProperty] string _description = string.Empty;

    public readonly PipelineStepsCollection Steps;
    public readonly GsLogMonitoringSettingsWrapper GsLogMonitoringSettings;

    readonly PipelineProfile _profile;

    public PipelineProfileWrapper(PipelineProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile), "Pipeline profile cannot be null.");

        Name = profile.Name;
        Description = profile.Description;

        GsLogMonitoringSettings = new(profile.GsLogMonitoringSettings);

        Steps = new PipelineStepsCollection(profile.Steps.Select(step => new PipelineStepWrapper(step)));
    }

    public PipelineProfile ToOriginal()
    {
        _profile.UpdateName(Name);
        _profile.UpdateDescription(Description);
        _profile.UpdateGsLogMonitoringSettings(GsLogMonitoringSettings.ToOriginal());

        return _profile;
    }
}