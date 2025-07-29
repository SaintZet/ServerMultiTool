using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class SqlExecutionSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty]
    private bool _enable;

    [ObservableProperty]
    private string _connectionString = string.Empty;

    [ObservableProperty]
    private string _pathToSqlScript = string.Empty;

    public SqlExecutionSettingsWrapper(SqlExecutionSettings settings)
    {
        Enable = settings.Enable;
        ConnectionString = settings.ConnectionString ?? string.Empty;
        PathToSqlScript = settings.PathToSqlScript ?? string.Empty;
    }

    public SqlExecutionSettings ToSqlExecutionSettings() => new()
    {
        Enable = Enable,
        ConnectionString = ConnectionString,
        PathToSqlScript = PathToSqlScript
    };
}