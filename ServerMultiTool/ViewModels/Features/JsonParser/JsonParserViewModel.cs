using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Common.Interfaces;
using ServerMultiTool.ViewModels.Components.GeneralInfo;

namespace ServerMultiTool.ViewModels.Features.JsonParser;

public partial class JsonParserViewModel : BaseViewModel, IPage
{
    #region Observable Properties

    public string Title => PageNames.JsonParserPage;

    [ObservableProperty] private GeneralInfoViewModel _generalInfo = null!;

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