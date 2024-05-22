using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class SqlExecutionService : PipelineOperation
{
    private readonly SqlExecutionSettings _settings;

    public SqlExecutionService(SqlExecutionSettings settings) => 
        _settings = settings;

    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        var sqlScript = await File.ReadAllTextAsync(_settings.PathToSqlScript);
        var connectionString = _settings.ConnectionString;
        
        var count = await ExecuteSqlScript(sqlScript, connectionString);
        Logger.LogInfo($"Affected row count {count}");

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