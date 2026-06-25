using ServerMultiTool.ViewModels.Common.BaseClasses;

namespace ServerMultiTool.ViewModels.Windows;

public sealed class NavigationMenuItemViewModel(
    string key,
    string title,
    object? icon,
    string? group = null,
    string? badge = null,
    string? featureFlag = null) : BaseViewModel
{
    public string Key { get; } = key;

    public string Title { get; } = title;

    public object? Icon { get; } = icon;

    public string? Group { get; } = group;

    public string? Badge { get; } = badge;

    public string? FeatureFlag { get; } = featureFlag;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

