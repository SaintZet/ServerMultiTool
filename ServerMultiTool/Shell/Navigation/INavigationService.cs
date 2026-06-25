using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace ServerMultiTool.Shell.Navigation;

public interface INavigationService : INotifyPropertyChanged
{
    IReadOnlyList<IPageDescriptor> RegisteredPages { get; }
    string? CurrentPageKey { get; }
    Page? CurrentPage { get; }

    void NavigateTo(string pageKey, object? parameter = null);
    bool TryNavigateTo(string pageKey, object? parameter = null);

    void NavigateTo(PageRoute route, object? parameter = null);
    bool TryNavigateTo(PageRoute route, object? parameter = null);
}

