using ServerMultiTool.Model.Services.Pipeline;
using ServerMultiTool.Model.Services.Settings;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Management;

public class PipelineProfileManager
{
    public List<PipelineProfileWrapper> PipelineProfiles { get; private set; } = [];
    public PipelineProfileWrapper SelectedProfile { get; private set; } = null!;

    public event EventHandler? ProfilesChanged;

    public PipelineProfileManager()
    {
        PipelineProfilesService.PipelineProfilesChanged += (_, _) => ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadProfiles(string selectedProfileName)
    {
        PipelineProfiles = PipelineProfilesService.PipelineProfiles.Select(profile => new PipelineProfileWrapper(profile)).ToList();

        SelectedProfile = GetSelectedProfile(selectedProfileName) ?? throw new InvalidOperationException("No profiles available.");
    }

    private PipelineProfileWrapper? GetSelectedProfile(string profileName)
    {
        return PipelineProfiles.FirstOrDefault(x => x.Name == profileName) ?? PipelineProfiles.FirstOrDefault();
    }

    public void UpdateProfile(PipelineProfileWrapper profile)
    {
        var appSettings = AppSettingsService.AppSettings;
        appSettings.CurrentPipelineProfileName = profile.Name;
        AppSettingsService.SaveAppSettings(appSettings);
    }
}