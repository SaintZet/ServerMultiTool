using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ServerMultiTool.Views.Pages;
using ServerMultiTool.Views.Themes;

namespace ServerMultiTool.Views;

public partial class MainWindow : Window
{
    private readonly Page _deployPage;
    private readonly Page _jsonParserPage;

    public MainWindow()
    {
        InitializeComponent();
        
        _deployPage = new CICDPipelineView();
        _jsonParserPage = new JsonParserView();
        
        FrameContent.Navigate(_deployPage);
        rdDeploy.IsChecked = true;
    }
    
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton is MouseButton.Left) 
            DragMove();
    }
    
    private void Themes_Click(object sender, RoutedEventArgs e)
    {
        var theme = Themes.IsChecked is true ? ThemesController.ThemeTypes.Dark : ThemesController.ThemeTypes.Light;
        ThemesController.ChangeTheme(theme);
    }
    
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void btnRestore_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState is WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
    }

    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    private void rbDeploy_Click(object sender, RoutedEventArgs e)
    {
        FrameContent.Navigate(_deployPage);
    }

    private void rdJsonParser_Click(object sender, RoutedEventArgs e)
    {
        FrameContent.Navigate(_jsonParserPage);
    }

    private void rdNotifications_OnClick(object sender, RoutedEventArgs e)
    {
        FrameContent.Navigate(_jsonParserPage);
    }

    private void rdSettings_OnClick(object sender, RoutedEventArgs e)
    {
        FrameContent.Navigate(_jsonParserPage);
    }
}