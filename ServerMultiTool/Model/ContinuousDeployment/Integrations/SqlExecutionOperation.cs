using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations
{
    public class SqlExecutionOperation(string name) : BasePipelineOperation(name)
    {
        public string PathToSqlScript { get; private set; } = string.Empty;
        public string ConnectionString { get; private set; } = string.Empty;

        public SqlExecutionOperation UpdatePathToSqlScript(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path to SQL script cannot be null or empty.", nameof(path));

            PathToSqlScript = path;
            return this;
        }

        public SqlExecutionOperation UpdateConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            ConnectionString = connectionString;
            return this;
        }

        protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(PathToSqlScript))
            {
                Logger.LogErrorWithPublish($"File not found: {PathToSqlScript}");
                return PipelineOperationResult.Failure;
            }

            var sqlScript = await File.ReadAllTextAsync(PathToSqlScript, cancellationToken);
            var count = await ExecuteSqlScript(sqlScript, ConnectionString, cancellationToken);

            Logger.LogInfoWithPublish($"Affected row count {count}");

            return PipelineOperationResult.Success;
        }

        private static async Task<int> ExecuteSqlScript(string script, string connectionString, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(script, connection);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
