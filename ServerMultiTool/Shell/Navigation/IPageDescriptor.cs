using System;
using System.Windows.Controls;

namespace ServerMultiTool.Shell.Navigation;

public interface IPageDescriptor
{
    PageRoute Route { get; }
    string Key { get; }
    string Title { get; }
    string IconResourceKey { get; }
    int Order { get; }
    bool ShowInMenu { get; }
    MenuPresentationMetadata Menu { get; }

    Page CreatePage(IServiceProvider serviceProvider);
}

