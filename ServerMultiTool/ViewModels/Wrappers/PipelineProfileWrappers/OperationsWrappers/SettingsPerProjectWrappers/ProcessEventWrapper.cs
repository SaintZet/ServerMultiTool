using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Contracts.BaseClasses;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers.SettingsPerProjectWrappers;

public partial class ProcessEventWrapper : BaseObservableWrapper
{
    [ObservableProperty] private string _path;
    [ObservableProperty] private string? _arguments;


    public ProcessEventWrapper(ProcessEvent processEvent)
    {
        Path = processEvent.Path;
        Arguments = processEvent.Arguments;
    }

    public ProcessEventWrapper(ProcessEventWrapper processEvent)
    {
        Path = processEvent.Path;
        Arguments = processEvent.Arguments;
    }

    public ProcessEvent ToProcessEvent() =>
        new() { Path = Path, Arguments = Arguments };

    public ProcessEventWrapper Clone() =>
        new(this);
}