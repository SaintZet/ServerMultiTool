using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class SqlExecutionService(SqlExecutionSettings settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        if (string.IsNullOrEmpty(settings.PathToSqlScript) || string.IsNullOrEmpty(settings.ConnectionString))
            return OperationResult.Cancelled;

        var sqlScript = await File.ReadAllTextAsync(settings.PathToSqlScript);
        var count = await ExecuteSqlScript(sqlScript, settings.ConnectionString);
        
        Logger.LogInfoWithPublish($"Affected row count {count}");

        return OperationResult.Success;
    }
    
    private static async Task<int> ExecuteSqlScript(string script, string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(script, connection);
        return await command.ExecuteNonQueryAsync();
    }
}