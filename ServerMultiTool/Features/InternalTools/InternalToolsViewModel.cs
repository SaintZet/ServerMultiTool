using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Shared.Components.GeneralInfo;
using ServerMultiTool.ViewModels.Common.BaseClasses;

namespace ServerMultiTool.Features.InternalTools;

public partial class InternalToolsViewModel : BaseViewModel
{
    public string Title => "Internal Tools";

    [ObservableProperty]
    private GeneralInfoViewModel _generalInfo;

    public InternalToolsViewModel(GeneralInfoViewModel generalInfo)
    {
        _generalInfo = generalInfo;
    }
}

