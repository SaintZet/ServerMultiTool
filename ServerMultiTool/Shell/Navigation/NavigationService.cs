using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ServerMultiTool.ViewModels.Common.BaseClasses;

namespace ServerMultiTool.Shell.Navigation;

public sealed class NavigationService : BaseViewModel, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyDictionary<string, IPageDescriptor> _descriptorByKey;
    private readonly Dictionary<string, Page> _pageCache = [];

    private Page? _currentPage;
    public Page? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    private string? _currentPageKey;
    public string? CurrentPageKey
    {
        get => _currentPageKey;
        private set => SetProperty(ref _currentPageKey, value);
    }

    public IReadOnlyList<IPageDescriptor> RegisteredPages { get; }

    public NavigationService(IEnumerable<IPageDescriptor> descriptors, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisteredPages = descriptors
            .OrderBy(descriptor => descriptor.Order)
            .ThenBy(descriptor => descriptor.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        _descriptorByKey = RegisteredPages.ToDictionary(descriptor => descriptor.Key, StringComparer.OrdinalIgnoreCase);
    }

    public void NavigateTo(string pageKey, object? parameter = null)
    {
        if (TryNavigateTo(pageKey, parameter))
            return;

        throw new InvalidOperationException($"Page with key '{pageKey}' is not registered.");
    }

    public bool TryNavigateTo(string pageKey, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(pageKey))
            return false;

        if (_descriptorByKey.TryGetValue(pageKey, out var descriptor) is false)
            return false;

        var page = ResolvePage(descriptor);

        if (page.DataContext is INavigationAware navigationAware)
            navigationAware.OnNavigatedTo(parameter);

        CurrentPage = page;
        CurrentPageKey = descriptor.Key;

        return true;
    }

    public void NavigateTo(PageRoute route, object? parameter = null)
    {
        NavigateTo(route.Key, parameter);
    }

    public bool TryNavigateTo(PageRoute route, object? parameter = null)
    {
        return route is not null && TryNavigateTo(route.Key, parameter);
    }

    private Page ResolvePage(IPageDescriptor descriptor)
    {
        if (_pageCache.TryGetValue(descriptor.Key, out var cachedPage))
            return cachedPage;

        var page = descriptor.CreatePage(_serviceProvider);
        _pageCache[descriptor.Key] = page;

        return page;
    }
}

