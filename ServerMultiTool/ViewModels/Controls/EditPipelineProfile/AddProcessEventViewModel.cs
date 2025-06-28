using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile;

public partial class AddProcessEventViewModel : ObservableObject
{
    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _arguments = string.Empty;

    public ProcessEventWrapper ToProcessEventWrapper() => 
        new(new ProcessEvent { Path = Path, Arguments = Arguments });
}