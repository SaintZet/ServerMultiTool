using log4net;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Domain.Pipeline.Interfaces;
using ServerMultiTool.Model.Infrastructure.Settings;
using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.Services.Pipeline;

public static class PipelineProfilesService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PipelineProfilesService));
    private static IPipelineProfilesRepository? _repository;
    private static string _pathToProfilesDirectory = null!;

    // Keep static list for backward compatibility
    public static List<PipelineProfile> PipelineProfiles { get; private set; } = [];

    // Keep static event for backward compatibility
    public static event EventHandler? PipelineProfilesChanged;

    // Initialize the repository
    private static IPipelineProfilesRepository Repository
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

    private static void OnRepositoryProfilesChanged(object? sender, EventArgs e)
    {
        PipelineProfiles = Repository.GetAll();
        PipelineProfilesChanged?.Invoke(null, EventArgs.Empty);
    }

    public static List<PipelineProfile> LoadOrInitialize(string appDirectory)
    {
        _pathToProfilesDirectory = appDirectory;
        var profiles = Repository.LoadOrInitialize(appDirectory);
        PipelineProfiles = profiles;
        return profiles;
    }

    public static void SavePipelineProfiles(IEnumerable<PipelineProfile> profiles)
    {
        Repository.SaveAll(profiles);
        PipelineProfiles = Repository.GetAll();
        Log.Info($"{nameof(PipelineProfiles)} have been successfully saved.");
    }

    public static PipelineProfile GetProfileById(Guid id)
    {
        return Repository.GetById(id);
    }

    public static PipelineProfile GetProfileByName(string name)
    {
        return Repository.GetByName(name);
    }

    public static void UpdateProfile(PipelineProfile profile)
    {
        Repository.Update(profile);
        PipelineProfiles = Repository.GetAll();
    }

    public static void UpdateProfileField<TValue>(Guid profileId, string fieldPath, TValue value)
    {
        Repository.UpdateField(profileId, fieldPath, value);
        PipelineProfiles = Repository.GetAll();
    }

    public static void AddProfile(PipelineProfile profile)
    {
        Repository.Add(profile);
        PipelineProfiles = Repository.GetAll();
    }

    public static void DeleteProfile(Guid profileId)
    {
        Repository.Delete(profileId);
        PipelineProfiles = Repository.GetAll();
    }
}