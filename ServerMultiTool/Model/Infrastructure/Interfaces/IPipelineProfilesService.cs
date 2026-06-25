using System;
using System.Collections.Generic;
using ServerMultiTool.Model.Features.Pipeline.Profile;

namespace ServerMultiTool.Model.Infrastructure.Interfaces
{
    public interface IPipelineProfilesService
    {
        IReadOnlyList<PipelineProfile> GetAll();
        PipelineProfile? GetById(Guid id);
        PipelineProfile? GetByName(string name);

        PipelineProfile Add(PipelineProfile profile);     // присваивает Id/имя файла при необходимости
        void Update(PipelineProfile profile);             // по Id
        void UpdateField<T>(Guid profileId, string fieldPath, T value); // "Root.Nested.Prop"
        bool Delete(Guid profileId);

        void SaveAll(IEnumerable<PipelineProfile> profiles);

        event EventHandler? ProfilesChanged;
    }
}
