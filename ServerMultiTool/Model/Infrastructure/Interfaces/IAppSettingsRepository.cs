using ServerMultiTool.Model.Infrastructure.Settings;

namespace ServerMultiTool.Model.Infrastructure.Interfaces;

public interface IAppSettingsRepository : IRepository<AppSettings>
{
    AppSettings LoadOrInitialize();

    AppSettings LoadOrInitialize(string appSettingsDirectory);
}