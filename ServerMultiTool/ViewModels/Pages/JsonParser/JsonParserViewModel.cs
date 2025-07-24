using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;

namespace ServerMultiTool.ViewModels.Pages.JsonParser;

public partial class JsonParserViewModel : BaseViewModel, IPage
{
    #region Observable Properties

    [ObservableProperty]
    private GeneralInfoViewModel _generalInfo = null!;

    #endregion

    #region Private Fields

    #endregion

    #region Collections

    #endregion

    #region Constructors

    public JsonParserViewModel()
    {
        // Initialize any necessary properties or services here
    }

    #endregion

    #region Commands
    // Add commands for JSON parsing functionality here
    #endregion

    #region Methods
    // Add methods for JSON parsing functionality here
    #endregion
}