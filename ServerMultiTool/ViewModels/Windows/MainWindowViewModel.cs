using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.ViewModels.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.JsonParser;
using ServerMultiTool.ViewModels.Pages.Pipeline;
using ServerMultiTool.ViewModels.Pages.Settings;
using ServerMultiTool.Views.Pages;
using ServerMultiTool.Views.Themes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ServerMultiTool.ViewModels.Windows
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly Page _pipelinePage;
        private readonly Page _jsonParserPage;
        private readonly Page _settingsPage;

        private Page _currentPage;

        public Page CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        private string _selectedMenu = PageNames.PipelinePage;
        public string SelectedMenu
        {
            get => _selectedMenu;
            private set => SetProperty(ref _selectedMenu, value);
        }

        public MainWindowViewModel()
        {
            var generalInfo = new GeneralInfoViewModel();

            var settingsViewModel = new SettingsViewModel { GeneralInfo = generalInfo };
            _settingsPage = new SettingsView(settingsViewModel);

            _pipelinePage = new PipelineView(new PipelineViewModel
            {
                GeneralInfo = generalInfo,
                NavigateToSettingsAction = (tabKey, param) =>
                {
                    if (string.IsNullOrEmpty(tabKey))
                        return;


                    settingsViewModel.SelectedTabKey = tabKey;

                    if (tabKey is SetiignsPageTabKeys.PipelineProfiles && string.IsNullOrEmpty(param) is false)
                        settingsViewModel.SelectedPipelineProfile = settingsViewModel.PipelineProfiles.First(x => x.Name == param);

                    Navigate(PageNames.SettingsPage);
                },
            });

            _jsonParserPage = new JsonParserView(new JsonParserViewModel { GeneralInfo = generalInfo });

            _currentPage = _pipelinePage;
        }

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
    }
}