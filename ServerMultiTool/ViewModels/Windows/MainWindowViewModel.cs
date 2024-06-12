using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.Views.Pages;
using ServerMultiTool.Views.Themes;

namespace ServerMultiTool.ViewModels.Windows
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly Page _pipelinePage;
        private readonly Page _jsonParserPage;

        public MainWindowViewModel()
        {
            var generalInfoViewModel = new GeneralInfoViewModel();
            
            _pipelinePage = new PipelineView(generalInfoViewModel);
            _jsonParserPage = new JsonParserView(generalInfoViewModel);

            _currentPage = _pipelinePage;
        }

        private Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        [RelayCommand]
        private void Navigate(string pageName)
        {
            CurrentPage = pageName switch
            {
                "Pipeline" => _pipelinePage,
                "JsonParser" => _jsonParserPage,
                _ => CurrentPage
            };
        }

        [RelayCommand]
        private void Close()
        {
            Application.Current.MainWindow?.Close();
        }

        [RelayCommand]
        private void Restore()
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }

        [RelayCommand]
        private void Minimize()
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private void ChangeTheme()
        {
            var theme = ThemesController.ThemeTypes.Light;
            
            if (ThemesController.CurrentTheme == ThemesController.ThemeTypes.Light)
                theme = ThemesController.ThemeTypes.Dark;
            
            ThemesController.ChangeTheme(theme);
        }
    }
}