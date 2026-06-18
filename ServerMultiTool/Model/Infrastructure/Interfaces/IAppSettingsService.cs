using ServerMultiTool.Model.Common;
using System;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Infrastructure.Interfaces
{
    public interface IAppSettingsService
    {
        AppSettings Get();
        Task<AppSettings> GetAsync();
        void Save(AppSettings settings);
        Task SaveAsync(AppSettings settings);
        void UpdateField<T>(string fieldName, T value);
        event EventHandler<AppSettings>? Changed;
    }
}
