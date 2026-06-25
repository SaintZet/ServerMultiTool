using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ServerMultiTool.Features.Pipeline.Presentation;
using ServerMultiTool.Shell.Navigation;
using ServerMultiTool.ViewModels.Windows;
using Xunit;

namespace ServerMultiTool.NavigationTests;

public sealed class NavigationServiceTests
{
    [StaFact]
    public void TryNavigateTo_ReturnsFalse_ForUnknownKey()
    {
        var sut = CreateNavigationService();

        var result = sut.TryNavigateTo("Unknown");

        Assert.False(result);
        Assert.Null(sut.CurrentPage);
        Assert.Null(sut.CurrentPageKey);
    }

    [StaFact]
    public void MainWindowViewModel_NavigatesToStartupRoute_WhenCurrentPageNotSet()
    {
        var descriptors = CreateDescriptors();
        var sut = new NavigationService(descriptors, new TestServiceProvider());

        _ = new MainWindowViewModel(descriptors, sut, new ShellOptions { StartupRoute = AppRoutes.Settings });

        Assert.Equal(AppRoutes.Settings.Key, sut.CurrentPageKey);
    }

    [StaFact]
    public void NavigateTo_UsesCachedPageInstance_ForSameKey()
    {
        var sut = CreateNavigationService();

        sut.NavigateTo(AppRoutes.Pipeline.Key);
        var firstPage = sut.CurrentPage;
        sut.NavigateTo(AppRoutes.Pipeline.Key);
        var secondPage = sut.CurrentPage;

        Assert.NotNull(firstPage);
        Assert.Same(firstPage, secondPage);
    }

    [StaFact]
    public void NavigateTo_TypedRoute_PassesTypedRequestToNavigationAware()
    {
        var descriptor = new TestNavigationAwareDescriptor(AppRoutes.Settings.Key, AppRoutes.Settings.Title);
        var sut = new NavigationService([descriptor], new TestServiceProvider());
        var request = new TypedNavigationRequest("pipeline-profiles", "ProfileA");

        sut.NavigateTo(AppRoutes.Settings, request);

        Assert.NotNull(sut.CurrentPage);
        Assert.Equal(AppRoutes.Settings.Key, sut.CurrentPageKey);

        var aware = Assert.IsType<TestNavigationAwareViewModel>(sut.CurrentPage!.DataContext);
        Assert.Same(request, aware.LastParameter);
    }

    private static NavigationService CreateNavigationService()
    {
        var descriptors = CreateDescriptors();
        return new NavigationService(descriptors, new TestServiceProvider());
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

        private int _createCount;
        private Page? _cached;

        public Page CreatePage(IServiceProvider serviceProvider)
        {
            _createCount++;
            _cached ??= new Page { DataContext = new object() };
            return _cached;
        }
    }

    private sealed class TestNavigationAwareDescriptor(string key, string title) : IPageDescriptor
    {
        public PageRoute Route { get; } = new(key, title);
        public string Key { get; } = key;
        public string Title { get; } = title;
        public string IconResourceKey { get; } = string.Empty;
        public int Order { get; } = 0;
        public bool ShowInMenu { get; } = true;
        public MenuPresentationMetadata Menu { get; } = new();

        public Page CreatePage(IServiceProvider serviceProvider)
            => new() { DataContext = new TestNavigationAwareViewModel() };
    }

    private sealed class TestNavigationAwareViewModel : INavigationAware
    {
        public object? LastParameter { get; private set; }

        public void OnNavigatedTo(object? parameter)
        {
            LastParameter = parameter;
        }
    }

    private sealed record TypedNavigationRequest(string TabKey, string ProfileName);
}

