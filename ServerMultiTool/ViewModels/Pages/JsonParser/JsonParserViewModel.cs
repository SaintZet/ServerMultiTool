using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;

namespace ServerMultiTool.ViewModels.Pages.JsonParser;

public partial class JsonParserViewModel : BaseViewModel
{
    private readonly GeneralInfoViewModel _generalInfo = null!;

    public GeneralInfoViewModel GeneralInfo
    {
        get => _generalInfo;
        init => SetProperty(ref _generalInfo, value);
    }
}