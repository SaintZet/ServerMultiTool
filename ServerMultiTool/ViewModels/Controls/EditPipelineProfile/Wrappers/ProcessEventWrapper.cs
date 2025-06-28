using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

public partial class ProcessEventWrapper : BaseObservableWrapper
{
    [ObservableProperty] private string _path;
    [ObservableProperty] private string? _arguments;


    public ProcessEventWrapper(ProcessEvent processEvent)
    {
        Path = processEvent.Path;
        Arguments = processEvent.Arguments;
    }

    public ProcessEvent ToProcessEvent() => 
        new() { Path = Path, Arguments = Arguments };
}