using ServerMultiTool.Model.ContinuousIntegration.GameServerLogs;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServerMultiTool.Model.Pipeline.Profiles;

[Serializable]
public class PipelineProfile
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Name { get; private set; }

    public string Description { get; private set; }

    public GsLogMonitoringSettings GsLogMonitoringSettings { get; private set; } = new GsLogMonitoringSettings();

    public IReadOnlyList<PipelineStep> Steps => _steps;

    private readonly List<PipelineStep> _steps;

    public PipelineProfile(string name, string desctiption)
    {
        Name = name;
        Description = desctiption;

        _steps = [];
    }

    [JsonConstructor]
    public PipelineProfile(string name, string desctiption, List<PipelineStep> steps)
    {
        Name = name;
        Description = desctiption;

        _steps = steps ?? [];
    }

    public PipelineProfile AddStep(PipelineStep step)
    {
        _steps.Add(step);
        return this;
    }

    public PipelineProfile RemoveStep(PipelineStep step)
    {
        _steps.Remove(step);
        return this;
    }

    public PipelineProfile UpdateName(string name)
    {
        Name = name;
        return this;
    }

    public PipelineProfile UpdateDescription(string description)
    {
        Description = description;
        return this;
    }

    public PipelineProfile UpdateGsLogMonitoringSettings(GsLogMonitoringSettings settings)
    {
        GsLogMonitoringSettings = settings;
        return this;
    }
}