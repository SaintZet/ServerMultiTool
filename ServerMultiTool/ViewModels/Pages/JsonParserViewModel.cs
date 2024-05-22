using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;

namespace ServerMultiTool.ViewModels.Pages;

public class JsonParserViewModel : BaseViewModel
{
    private GeneralInfoViewModel _generalInfo;

    public GeneralInfoViewModel GeneralInfo
    {
        get => _generalInfo;
        set => SetProperty(ref _generalInfo, value);
    }
}