using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Common.BaseClasses;
using ServerMultiTool.Views.Themes;

namespace ServerMultiTool.ViewModels.Windows
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        #region Observable Properties

        [ObservableProperty]
        private Page? _currentPage;

        [ObservableProperty]
        private string _selectedMenu = string.Empty;

        public ObservableCollection<NavigationMenuItemViewModel> MenuItems { get; }

        #endregion

        #region Private Fields

        private readonly INavigationService _navigationService;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IEnumerable<IPageDescriptor> pageDescriptors,
            INavigationService navigationService,
            ShellOptions shellOptions)
        {
            _navigationService = navigationService;
            _navigationService.PropertyChanged += OnNavigationServicePropertyChanged;

            MenuItems = new ObservableCollection<NavigationMenuItemViewModel>(
                pageDescriptors
                    .Where(descriptor => descriptor.ShowInMenu)
                    .OrderBy(descriptor => descriptor.Order)
                    .ThenBy(descriptor => descriptor.Title, StringComparer.OrdinalIgnoreCase)
                    .Select(descriptor => new NavigationMenuItemViewModel(
                        descriptor.Key,
                        descriptor.Title,
                        Application.Current?.TryFindResource(descriptor.IconResourceKey),
                        descriptor.Menu.Group,
                        descriptor.Menu.Badge,
                        descriptor.Menu.FeatureFlag)));

            SynchronizeWithNavigationService();

            if (string.IsNullOrWhiteSpace(_navigationService.CurrentPageKey))
            {
                if (_navigationService.TryNavigateTo(shellOptions.StartupRoute) is false)
                    _navigationService.NavigateTo(shellOptions.StartupPageKey);
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void Navigate(string pageName)
        {
            _navigationService.NavigateTo(pageName);
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

        private void OnNavigationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(INavigationService.CurrentPage) or nameof(INavigationService.CurrentPageKey))
                SynchronizeWithNavigationService();
        }

        private void SynchronizeWithNavigationService()
        {
            CurrentPage = _navigationService.CurrentPage;
            SelectedMenu = _navigationService.CurrentPageKey ?? string.Empty;

            foreach (var menuItem in MenuItems)
                menuItem.IsSelected = string.Equals(menuItem.Key, SelectedMenu, StringComparison.OrdinalIgnoreCase);
        }
    }
}
