using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

public partial class ProcessEventWrapper : ObservableObject
{
    [ObservableProperty] 
    private string _path;

    [ObservableProperty] 
    private string _arguments;
    
    public ProcessEventWrapper()
    {
    }


    public ProcessEventWrapper(ProcessEvent processEvent)
    {
        Path = processEvent.Path;
        Arguments = processEvent.Arguments;
    }

    public ProcessEvent ToProcessEvent() => new()
    {
        Path = Path,
        Arguments = Arguments
    };
}