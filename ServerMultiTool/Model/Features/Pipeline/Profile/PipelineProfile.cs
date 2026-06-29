using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ServerMultiTool.Model.Features.Pipeline.Step;

namespace ServerMultiTool.Model.Features.Pipeline.Profile;

[Serializable]
public class PipelineProfile
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Name { get; private set; }

    public string Description { get; private set; }

    public List<PipelineStep> Steps { get; private set; }

    public PipelineProfile(string name, string description)
    {
        Name = name;
        Description = description;
        Steps = [];
    }

    [JsonConstructor]
    public PipelineProfile(Guid id, string name, string description, List<PipelineStep> steps)
    {
        Id = id;
        Name = name;
        Description = description;
        Steps = steps ?? [];
    }

    public PipelineProfile AddStep(PipelineStep step)
    {
        Steps.Add(step);
        return this;
    }

    public PipelineProfile RemoveStep(PipelineStep step)
    {
        Steps.Remove(step);
        return this;
    }

    public PipelineProfile UpdateSteps(List<PipelineStep> steps)
    {
        Steps = steps ?? [];
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
}
