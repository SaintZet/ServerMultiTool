using System.Windows.Controls;
namespace ServerMultiTool.Features.Settings.Presentation;

public partial class SettingsView : Page
{
    public SettingsView(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}

