using ServerMultiTool.Models.Settings.Contracts;
using ServerMultiTool.Models.Settings.Global.Data;

namespace ServerMultiTool.Models.Settings.Global.Contracts;

public interface IGlobalSettingsService : ISettingsService
{
    GlobalSettings GlobalSettings { get; }
}