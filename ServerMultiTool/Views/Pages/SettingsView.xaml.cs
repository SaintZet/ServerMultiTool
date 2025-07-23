using System.Windows.Controls;
using SettingsViewModel = ServerMultiTool.ViewModels.Pages.Settings.SettingsViewModel;

namespace ServerMultiTool.Views.Pages;

public partial class SettingsView : Page
{
    public SettingsView(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}