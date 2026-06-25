using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ServerMultiTool.Shell.Navigation;

public sealed class PageDescriptor<TPage>(
    string key,
    string title,
    string iconResourceKey,
    int order,
    bool showInMenu = true,
    MenuPresentationMetadata? menu = null) : IPageDescriptor
    where TPage : Page
{
    public PageRoute Route { get; } = new(key, title);

    public string Key { get; } = key;

    public string Title { get; } = title;

    public string IconResourceKey { get; } = iconResourceKey;

    public int Order { get; } = order;

    public bool ShowInMenu { get; } = showInMenu;

    public MenuPresentationMetadata Menu { get; } = menu ?? new MenuPresentationMetadata();

    public Page CreatePage(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<TPage>();
    }
}

