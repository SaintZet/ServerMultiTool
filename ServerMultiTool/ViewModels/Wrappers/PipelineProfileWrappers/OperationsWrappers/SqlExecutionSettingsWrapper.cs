using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class SqlExecutionSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty] private bool _enable;
    [ObservableProperty] private string _connectionString = string.Empty;
    [ObservableProperty] private string _pathToSqlScript = string.Empty;

    public SqlExecutionSettingsWrapper(SqlExecutionSettings settings)
    {
        Enable = settings.Enable;
        ConnectionString = settings.ConnectionString ?? string.Empty;
        PathToSqlScript = settings.PathToSqlScript ?? string.Empty;
    }

    public SqlExecutionSettings ToSqlExecutionSettings()
    {
        return new SqlExecutionSettings
        {
            Enable = Enable,
            ConnectionString = ConnectionString,
            PathToSqlScript = PathToSqlScript
        };
    }

    partial void OnEnableChanged(bool value)
    {
        OnPropertyChanged(string.Empty);
    }

    partial void OnConnectionStringChanged(string value)
    {
        OnPropertyChanged(string.Empty);
    }

    partial void OnPathToSqlScriptChanged(string value)
    {
        OnPropertyChanged(string.Empty);
    }
}