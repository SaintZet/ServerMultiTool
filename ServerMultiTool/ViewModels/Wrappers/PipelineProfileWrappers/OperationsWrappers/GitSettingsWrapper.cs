using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;

public partial class GitSettingsWrapper : BaseObservableWrapper
{
    [ObservableProperty] private bool _enable;
    [ObservableProperty] private bool _shouldPull;

    public GitSettingsWrapper(GitSettings settings)
    {
        Enable = settings.Enable;
        ShouldPull = settings.ShouldPull;
    }

    public GitSettings ToGitSettings()
    {
        return new GitSettings
        {
            Enable = Enable,
            ShouldPull = ShouldPull
        };
    }

    partial void OnEnableChanged(bool value)
    {
        OnPropertyChanged(string.Empty);
    }

    partial void OnShouldPullChanged(bool value)
    {
        OnPropertyChanged(string.Empty);
    }
}