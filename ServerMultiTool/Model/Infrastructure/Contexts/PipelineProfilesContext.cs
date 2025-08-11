using log4net;
using ServerMultiTool.Model.Features.Pipeline.Profile;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using ServerMultiTool.Model.Infrastructure.Repositories;
using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.Infrastructure.Services.Pipeline;

public class PipelineProfilesContext
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PipelineProfilesContext));
    private static readonly object _lockObject = new();
    private static PipelineProfilesContext? _instance;

    private IPipelineProfilesRepository? _repository;
    private string _pathToProfilesDirectory = null!;

    private PipelineProfilesContext() { }

    public static PipelineProfilesContext Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    _instance ??= new PipelineProfilesContext();
                }
            }
            return _instance;
        }
    }

    public List<PipelineProfile> PipelineProfiles { get; private set; } = [];

    public event EventHandler? PipelineProfilesChanged;

    private IPipelineProfilesRepository Repository
    {
        get
        {
            if (_repository == null)
            {
                if (string.IsNullOrEmpty(_pathToProfilesDirectory))
                    throw new InvalidOperationException("Profiles directory is not initialized. Call LoadOrInitialize first.");

                _repository = new PipelineProfilesRepository(_pathToProfilesDirectory, Log);
                _repository.PipelineProfilesChanged += OnRepositoryProfilesChanged;
            }
            return _repository;
        }
    }

    private void OnRepositoryProfilesChanged(object? sender, EventArgs e)
    {
        PipelineProfiles = Repository.GetAll();
        PipelineProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<PipelineProfile> LoadOrInitialize(string appDirectory)
    {
        _pathToProfilesDirectory = appDirectory;
        var profiles = Repository.LoadOrInitialize(appDirectory);
        PipelineProfiles = profiles;
        return profiles;
    }

    public void SavePipelineProfiles(List<PipelineProfile> profiles)
    {
        Repository.SaveAll(profiles);
        PipelineProfiles = Repository.GetAll();
        Log.Info($"{nameof(PipelineProfiles)} have been successfully saved.");
    }

    public PipelineProfile GetProfileById(Guid id)
    {
        return Repository.GetById(id);
    }

    public PipelineProfile GetProfileByName(string name)
    {
        return Repository.GetByName(name);
    }

    public void UpdateProfile(PipelineProfile profile)
    {
        Repository.Update(profile);
        PipelineProfiles = Repository.GetAll();
    }

    public void UpdateProfileField<TValue>(Guid profileId, string fieldPath, TValue value)
    {
        Repository.UpdateField(profileId, fieldPath, value);
        PipelineProfiles = Repository.GetAll();
    }

    public void AddProfile(PipelineProfile profile)
    {
        Repository.Add(profile);
        PipelineProfiles = Repository.GetAll();
    }

    public void DeleteProfile(Guid profileId)
    {
        Repository.Delete(profileId);
        PipelineProfiles = Repository.GetAll();
    }
}