using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class SqlExecutionService(SqlExecutionSettings settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (string.IsNullOrEmpty(settings.PathToSqlScript) || string.IsNullOrEmpty(settings.ConnectionString))
            return OperationResult.Cancelled;

        if (!File.Exists(settings.PathToSqlScript))
        {
            Logger.LogErrorWithPublish($"File not found: {settings.PathToSqlScript}");
            return OperationResult.Failure;
        }
        
        var sqlScript = await File.ReadAllTextAsync(settings.PathToSqlScript, cancellationToken);
        var count = await ExecuteSqlScript(sqlScript, settings.ConnectionString, cancellationToken);
        
        Logger.LogInfoWithPublish($"Affected row count {count}");

        return OperationResult.Success;
    }
    
    private static async Task<int> ExecuteSqlScript(string script, string connectionString, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(script, connection);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}