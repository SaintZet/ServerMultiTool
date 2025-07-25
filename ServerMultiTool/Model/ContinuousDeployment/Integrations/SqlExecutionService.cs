using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class SqlExecutionService(SqlExecutionSettings settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        if (string.IsNullOrEmpty(settings.PathToSqlScript) || string.IsNullOrEmpty(settings.ConnectionString))
            return OperationResult.Failure;

        if (!File.Exists(settings.PathToSqlScript))
        {
            Logger.LogErrorWithPublish($"File not found: {settings.PathToSqlScript}");
            return OperationResult.Failure;
        }

        try
        {
            var sqlScript = await File.ReadAllTextAsync(settings.PathToSqlScript, cancellationToken);
            var count = await ExecuteSqlScript(sqlScript, settings.ConnectionString, cancellationToken);

            Logger.LogInfoWithPublish($"Affected row count {count}");

            return OperationResult.Success;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled;
        }
    }

    private static async Task<int> ExecuteSqlScript(string script, string connectionString, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(script, connection);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}