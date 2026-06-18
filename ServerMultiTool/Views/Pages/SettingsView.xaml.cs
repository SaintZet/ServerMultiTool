using ServerMultiTool.ViewModels.Pages.Settings;
using System.Windows.Controls;

namespace ServerMultiTool.Views.Pages;

public partial class SettingsView : Page
{
    public SettingsView(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}