using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.ViewModels.Common.Interfaces;
using ServerMultiTool.Shared.Components.GeneralInfo;

namespace ServerMultiTool.Features.JsonParser.Presentation;

public partial class JsonParserViewModel : BaseViewModel, IPage
{
    #region Observable Properties

    public string Title => AppRoutes.JsonParser.Key;

    [ObservableProperty] private GeneralInfoViewModel _generalInfo;

    #endregion

    #region Private Fields

    #endregion

    #region Collections

    #endregion

    #region Constructors

    public JsonParserViewModel(GeneralInfoViewModel generalInfo)
    {
        _generalInfo = generalInfo;
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



