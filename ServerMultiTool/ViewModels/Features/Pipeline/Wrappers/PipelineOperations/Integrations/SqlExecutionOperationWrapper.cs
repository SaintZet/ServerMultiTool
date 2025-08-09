using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class SqlExecutionOperationWrapper : PipelineOperationWrapper
{
    [ObservableProperty] string _connectionString = string.Empty;
    [ObservableProperty] string _pathToSqlScript = string.Empty;

    public override string Name => "SQL Execution";
    public override string Description => "Executes a SQL script against a specified database using the provided connection string.";

    readonly SqlExecutionOperation _operation;

    public SqlExecutionOperationWrapper(SqlExecutionOperation operation)
        : base(operation)
    {
        _operation = operation;

        ConnectionString = operation.ConnectionString;
        PathToSqlScript = operation.PathToSqlScript ?? string.Empty;
    }

    public override PipelineOperation ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        _operation.UpdateConnectionString(ConnectionString);
        _operation.UpdatePathToSqlScript(PathToSqlScript);

        return _operation;
    }
}