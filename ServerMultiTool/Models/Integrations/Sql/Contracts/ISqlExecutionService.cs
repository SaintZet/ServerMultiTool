using System.Threading.Tasks;
using ServerMultiTool.Models.Integrations.Sql.Data;

namespace ServerMultiTool.Models.Integrations.Sql.Contracts;

public interface ISqlExecutionService
{
    Task ExecuteSqlScriptAsync(SqlIntegrationSettings settings);
}