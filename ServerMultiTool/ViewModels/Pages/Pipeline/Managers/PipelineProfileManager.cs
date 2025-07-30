using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Managers;

public class PipelineProfileManager
{
    public List<PipelineProfile> PipelineProfiles { get; private set; } = [];
    public PipelineProfile SelectedProfile { get; private set; } = null!;

    public event EventHandler? ProfilesChanged;

    public PipelineProfileManager()
    {
        PipelineProfilesService.PipelineProfilesChanged += (_, _) => ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadProfiles(string selectedProfileName)
    {
        PipelineProfiles = PipelineProfilesService.PipelineProfiles;
        SelectedProfile = GetSelectedProfile(selectedProfileName) ?? throw new InvalidOperationException("No profiles available.");
    }

    private PipelineProfile? GetSelectedProfile(string profileName)
    {
        return PipelineProfiles.FirstOrDefault(x => x.Name == profileName) ?? PipelineProfiles.FirstOrDefault();
    }

    public static void UpdateProfile(PipelineProfile profile)
    {
        var appSettings = AppSettingsService.AppSettings;
        appSettings.CurrentPipelineProfileName = profile.Name;
        AppSettingsService.SaveAppSettings(appSettings);
    }
}