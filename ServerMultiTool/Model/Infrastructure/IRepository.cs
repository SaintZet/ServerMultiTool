using System.Threading.Tasks;

namespace ServerMultiTool.Model.Infrastructure;

public interface IRepository<T> where T : class
{
    T Get();

    void Update(T entity);

    void UpdateField<TValue>(string fieldPath, TValue value);

    Task<T> GetAsync();

    Task UpdateAsync(T entity);

    Task UpdateFieldAsync<TValue>(string fieldPath, TValue value);
}