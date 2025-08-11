using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class SqlExecutionOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "SQL Execution";

    [ObservableProperty] string _connectionString;
    [ObservableProperty] string _pathToSqlScript;

    readonly SqlExecutionOperation _operation;

    public SqlExecutionOperationWrapper(SqlExecutionOperation operation)
        : base(operation)
    {
        _operation = operation;

        ConnectionString = operation.ConnectionString;
        PathToSqlScript = operation.PathToSqlScript ?? string.Empty;
    }

    public override PipelineOperationBase ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        _operation.UpdateConnectionString(ConnectionString);
        _operation.UpdatePathToSqlScript(PathToSqlScript);

        return _operation;
    }
}