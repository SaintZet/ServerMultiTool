using System.Windows.Controls;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages;
using SettingsViewModel = ServerMultiTool.ViewModels.Pages.Settings.SettingsViewModel;

namespace ServerMultiTool.Views.Pages;

public partial class SettingsView : Page
{
    public SettingsView(GeneralInfoViewModel generalInfo)
    {
        var viewModel = new SettingsViewModel
        {
            GeneralInfo = generalInfo
        };
            
        DataContext = viewModel;
        InitializeComponent();
    }
}