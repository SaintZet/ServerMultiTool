using System.Collections.Generic;
using ServerMultiTool.Models.Settings.Contracts;
using ServerMultiTool.Models.Settings.Profiles.Data;

namespace ServerMultiTool.Models.Settings.Profiles.Contracts;

public interface ISettingsProfilesService : ISettingsService
{
    IEnumerable<SettingsProfile> GetProfilesSettings();
}