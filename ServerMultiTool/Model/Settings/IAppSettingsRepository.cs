using ServerMultiTool.Model.Infrastructure;

namespace ServerMultiTool.Model.Settings;

public interface IAppSettingsRepository : IRepository<AppSettings>
{
    AppSettings LoadOrInitialize();

    AppSettings LoadOrInitialize(string appSettingsDirectory);
}