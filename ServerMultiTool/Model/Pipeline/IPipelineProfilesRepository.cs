using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.Pipeline;

public interface IPipelineProfilesRepository
{
    List<PipelineProfile> GetAll();

    PipelineProfile GetById(Guid id);

    PipelineProfile GetByName(string name);

    void Add(PipelineProfile profile);

    void Update(PipelineProfile profile);

    void UpdateField<TValue>(Guid profileId, string fieldPath, TValue value);

    void Delete(Guid profileId);

    void SaveAll(IEnumerable<PipelineProfile> profiles);

    List<PipelineProfile> LoadOrInitialize(string appDirectory);

    event EventHandler PipelineProfilesChanged;
}