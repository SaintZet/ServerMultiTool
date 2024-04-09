using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Models.Settings.Global.Contracts;
using ServerMultiTool.Models.Settings.Global.Data;

namespace ServerMultiTool.Models.Settings.Global.Services;

public class GlobalSettingsService : IGlobalSettingsService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(GlobalSettingsService));
    public string PathToSettingsFolder { get; }

    private readonly string _pathToSettingFile;

    public GlobalSettings GlobalSettings { get; private set; }

    public GlobalSettingsService(string settingsFolder)
    {
        _pathToSettingFile = Path.Combine(settingsFolder, GlobalSettingsConstants.SettingsFileName);
        PathToSettingsFolder = settingsFolder;
    }
    
    public async Task LoadSettingsAsync()
    {
        if (File.Exists(_pathToSettingFile) is false)
            return;
        
        var json = await File.ReadAllTextAsync(_pathToSettingFile);
        GlobalSettings = JsonSerializer.Deserialize<GlobalSettings>(json);
        
        Log.Info($"Сеттинги успешно загружены.");
    }

    public async Task SaveSettingsAsync()
    {
        var json = JsonSerializer.Serialize(GlobalSettings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_pathToSettingFile, json);
    }

    public void UpdateSetting(string key, object value)
    {
        throw new NotImplementedException();
    }
}
