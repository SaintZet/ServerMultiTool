using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;

public class SqlExecutionService : ExecutionService
{
    public async Task<bool> ExecuteAsync(PipelineProfile pipeline)
    {
        var settings = pipeline.SqlExecutionSettings;
        if (settings.Enable is false)
        {
            Logger.LogInfo($"Sql Execution is disabled by {nameof(PipelineProfile)}.");
            return true;
        }
        
        try
        {
            var sqlScript = await File.ReadAllTextAsync(settings.PathToSqlScript);
            var connectionString = settings.ConnectionString;
            var count = await ExecuteSqlScript(sqlScript, connectionString);
            Logger.LogInfo($"Affected row count {count}");
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"An error occurred while executing the SQL script: \n{ex.Message}");
            
            return false;
        }
    }
    
    private static async Task<int> ExecuteSqlScript(string script, string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(script, connection);
        return await command.ExecuteNonQueryAsync();
    }
}