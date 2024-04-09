using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Models.Settings.Global.Contracts;
using ServerMultiTool.Models.Settings.Profiles.Contracts;
using ServerMultiTool.Models.Settings.Profiles.Data;

namespace ServerMultiTool.Models.Settings.Profiles.Services;

public class SettingsProfilesService : ISettingsProfilesService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(SettingsProfilesService));
    private readonly IGlobalSettingsService _globalSettingsService;
    
    private SettingsProfile _currentSettingsProfile;
    private IEnumerable<SettingsProfile>? _allProfileSettings;
    
    public string PathToSettingsFolder { get; }

    public SettingsProfilesService(IGlobalSettingsService globalSettingsService)
    {
        _globalSettingsService = globalSettingsService;

        var pathToDirectory = globalSettingsService.PathToSettingsFolder;
        PathToSettingsFolder = Path.Combine(pathToDirectory, SettingsProfilesConstants.SettingsFolderName);
    }

    public async Task LoadSettingsAsync()
    {
        if (Directory.Exists(PathToSettingsFolder) is false)
            throw new Exception(); //TODO

        var settings = new List<Profiles.Data.SettingsProfile>();
        
        foreach (var filePath in Directory.GetFiles(PathToSettingsFolder, SettingsProfilesConstants.SearchPattern))
        {
            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
            };
            var setting = JsonSerializer.Deserialize<Profiles.Data.SettingsProfile>(json, options);
            settings.Add(setting);
        }
        
        if (settings.Any() is false)
            throw new Exception(); //TODO
        
        _allProfileSettings = settings;
        
        Log.Info($"Сеттинги успешно загружены.");
    }

    public Task SaveSettingsAsync()
    {
        throw new NotImplementedException();
    }

    public void UpdateSetting(string key, object value)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<SettingsProfile> GetProfilesSettings() =>
        _allProfileSettings;
}