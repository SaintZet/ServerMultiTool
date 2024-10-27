using System.Windows.Controls;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages;

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