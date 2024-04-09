using System;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Microsoft.Data.SqlClient;
using ServerMultiTool.Models.Integrations.Sql.Contracts;
using ServerMultiTool.Models.Integrations.Sql.Data;

namespace ServerMultiTool.Models.Integrations.Sql.Services;

public class SqlExecutionService : ISqlExecutionService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(SqlExecutionService));
    
    public async Task ExecuteSqlScriptAsync(SqlIntegrationSettings settings)
    {
        if (settings.Enable is false)
            return;
        
        try
        {
            var sqlScript = await File.ReadAllTextAsync(settings.PathToSqlScript);
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(sqlScript, connection);
            var affectedRowCount = await command.ExecuteNonQueryAsync();
            Log.Info($"Affected row count {affectedRowCount}");
        }
        catch (Exception ex)
        {
            Log.Error($"Произошла ошибка при выполнении SQL скрипта:\n{ex.Message}");
            throw;
        }
    }
}