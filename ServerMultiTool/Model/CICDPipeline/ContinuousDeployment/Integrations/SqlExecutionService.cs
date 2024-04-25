using System;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Integrations;

public static class SqlExecutionService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(SqlExecutionService));
    
    public static async Task ExecuteAsync(PipelineProfile pipeline)
    {
        var settings = pipeline.SqlExecutionSettings;
        if (settings.Enable is false)
        {
            Log.Info($"Sql Execution is disabled by {nameof(PipelineProfile)}.");
            return;
        }
        
        try
        {
            var sqlScript = await File.ReadAllTextAsync(settings.PathToSqlScript);
            var connectionString = settings.ConnectionString;
            var count = await ExecuteSqlScript(sqlScript, connectionString);
            Log.Info($"Affected row count {count}");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while executing the SQL script: \n{ex.Message}");
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