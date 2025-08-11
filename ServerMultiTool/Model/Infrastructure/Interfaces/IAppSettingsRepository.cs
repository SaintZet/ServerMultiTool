using ServerMultiTool.Model.Common;

namespace ServerMultiTool.Model.Infrastructure.Interfaces;

public interface IAppSettingsRepository : IRepository<AppSettings>
{
    AppSettings LoadOrInitialize();

    AppSettings LoadOrInitialize(string appSettingsDirectory);
}