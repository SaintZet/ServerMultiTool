using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Domain.Pipeline.Interfaces;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class SqlExecutionOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    [ObservableProperty] string _connectionString = string.Empty;

    [ObservableProperty] string _pathToSqlScript = string.Empty;

    public string Name => "SQL Execution";
    public string Description => "Executes a SQL script against a specified database using the provided connection string.";

    readonly SqlExecutionOperation _operation;

    public SqlExecutionOperationWrapper(SqlExecutionOperation operation)
    {
        _operation = operation;

        ConnectionString = operation.ConnectionString;
        PathToSqlScript = operation.PathToSqlScript ?? string.Empty;
    }

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _operation.ExecuteAsync(cancellationToken);
    }

    public IPipelineOperation ToOriginal()
    {
        _operation.EnableOperation(Enabled);
        _operation.UpdateConnectionString(ConnectionString);
        _operation.UpdatePathToSqlScript(PathToSqlScript);

        return _operation;
    }

    public void UpdateSolutionDirectory(DirectoryModel directory)
    {
        _operation.UpdateSolutionDirectory(directory);
    }

    public void UpdateHttpDirectory(DirectoryModel directory)
    {
        _operation.UpdateHttpDirectory(directory);
    }
}