using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ServerMultiTool.Views.Pages;
using ServerMultiTool.Views.Themes;

namespace ServerMultiTool.Views;

public partial class MainWindow : Window
{
    private readonly Page _pipelinePage;
    private readonly Page _jsonParserPage;

    public MainWindow()
    {
        InitializeComponent();
        
        _pipelinePage = new PipelineView();
        _jsonParserPage = new JsonParserView();
        
        FrameContent.Navigate(_pipelinePage);
        rdPipeline.IsChecked = true;
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
    
    private void rbPipeline_Click(object sender, RoutedEventArgs e)
    {
        FrameContent.Navigate(_pipelinePage);
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