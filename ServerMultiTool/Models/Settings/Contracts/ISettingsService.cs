using System.Threading.Tasks;

namespace ServerMultiTool.Models.Settings.Contracts;

public interface ISettingsService
{
    string PathToSettingsFolder { get; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
    void UpdateSetting(string key, object value);
}