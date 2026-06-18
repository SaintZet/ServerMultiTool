using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.ViewModels.Common;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.Views.Pages;
using ServerMultiTool.Views.Themes;
using System.Windows;
using System.Windows.Controls;

namespace ServerMultiTool.ViewModels.Windows
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        #region Observable Properties

        [ObservableProperty]
        private Page _currentPage;

        [ObservableProperty]
        private string _selectedMenu = PageNames.PipelinePage;

        #endregion

        #region Private Fields

        private readonly Page _pipelinePage;
        private readonly Page _jsonParserPage;
        private readonly Page _settingsPage;

        #endregion

        #region Constructors

        public MainWindowViewModel(SettingsView settingsView, PipelineView pipelineView, JsonParserView jsonView)
        {
            _settingsPage = settingsView;
            _pipelinePage = pipelineView;
            _jsonParserPage = jsonView;

            _currentPage = _pipelinePage;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void Navigate(string pageName)
        {
            CurrentPage = pageName switch
            {
                PageNames.PipelinePage => _pipelinePage,
                PageNames.JsonParserPage => _jsonParserPage,
                PageNames.SettingsPage => _settingsPage,
                _ => CurrentPage
            };

            SelectedMenu = pageName;
        }

        [RelayCommand]
        private static void Close()
        {
            Application.Current.MainWindow?.Close();
        }

        [RelayCommand]
        private static void Restore()
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }

        [RelayCommand]
        private static void Minimize()
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private static void ChangeTheme()
        {
            var theme = ThemesController.ThemeTypes.Light;

            if (ThemesController.CurrentTheme == ThemesController.ThemeTypes.Light)
                theme = ThemesController.ThemeTypes.Dark;

            ThemesController.ChangeTheme(theme);
        }

        #endregion
    }
}