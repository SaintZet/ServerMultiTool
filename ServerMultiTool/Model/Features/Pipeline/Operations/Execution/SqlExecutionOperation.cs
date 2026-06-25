using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;

namespace ServerMultiTool.Model.Features.Pipeline.Operations.Execution
{
    public class SqlExecutionOperation(string name) : PipelineOperationBase(name)
    {
        public override OperationType OperationType => OperationType.SqlExecutionOperation;

        [JsonInclude] public string PathToSqlScript { get; private set; } = string.Empty;
        [JsonInclude] public string ConnectionString { get; private set; } = string.Empty;

        public SqlExecutionOperation UpdatePathToSqlScript(string path)
        {
            PathToSqlScript = path;
            return this;
        }

        public SqlExecutionOperation UpdateConnectionString(string connectionString)
        {
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

            Logger.LogInfoWithPublish($"Attempting to execute SQL script from {PathToSqlScript}");

            var sqlScript = await File.ReadAllTextAsync(PathToSqlScript, cancellationToken);
            var count = await ExecuteSqlScript(sqlScript, ConnectionString, cancellationToken);

            Logger.LogSuccessWithPublish($"{Name} has completed successfully. Affected row count {count}");

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
