using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Windows;
using Xunit;

namespace ServerMultiTool.UiAutomationTests;

public sealed class ShellNavigationSmokeTests
{
    [WpfFact]
    public void MainWindowXaml_ContainsFrameAndMenuAutomationIds()
    {
        var xamlPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "ServerMultiTool",
            "Views",
            "Windows",
            "MainWindowView.xaml"));

        var xaml = File.ReadAllText(xamlPath);

        Assert.Contains("AutomationProperties.AutomationId=\"ShellFrame\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"{Binding Key, StringFormat=NavMenu_{0}}\"", xaml, StringComparison.Ordinal);
    }

    [WpfFact]
    public void ShellNavigation_StartupAndMenuNavigationWork()
    {
        var descriptors = CreateDescriptors();
        var navigationService = new NavigationService(descriptors, new TestServiceProvider());
        var viewModel = new MainWindowViewModel(descriptors, navigationService, new ShellOptions { StartupRoute = AppRoutes.Pipeline });

        Assert.Equal(AppRoutes.Pipeline.Key, viewModel.SelectedMenu);

        viewModel.NavigateCommand.Execute(AppRoutes.Settings.Key);

        Assert.Equal(AppRoutes.Settings.Key, viewModel.SelectedMenu);
        Assert.NotNull(viewModel.CurrentPage);
    }

    private static IReadOnlyList<IPageDescriptor> CreateDescriptors()
    {
        return
        [
            new TestPageDescriptor(AppRoutes.Pipeline.Key, AppRoutes.Pipeline.Title),
            new TestPageDescriptor(AppRoutes.Settings.Key, AppRoutes.Settings.Title)
        ];
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class TestPageDescriptor(string key, string title) : IPageDescriptor
    {
        public PageRoute Route { get; } = new(key, title);
        public string Key { get; } = key;
        public string Title { get; } = title;
        public string IconResourceKey { get; } = string.Empty;
        public int Order { get; } = 0;
        public bool ShowInMenu { get; } = true;
        public MenuPresentationMetadata Menu { get; } = new();

        public Page CreatePage(IServiceProvider serviceProvider) => new();
    }
}






