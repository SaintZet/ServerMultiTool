using System.Windows;
using System.Windows.Controls;

namespace ServerMultiTool.Views.Behaviours;

public static class ScrollToEndBehavior
{
    public static readonly DependencyProperty AutoScrollToEndProperty =
        DependencyProperty.RegisterAttached(
             name: "AutoScrollToEnd", 
             propertyType:  typeof(bool),
             ownerType: typeof(ScrollToEndBehavior),
             defaultMetadata: new UIPropertyMetadata(false, OnAutoScrollToEndChanged));

    public static bool GetAutoScrollToEnd(DependencyObject obj) => 
        (bool)obj.GetValue(AutoScrollToEndProperty);

    public static void SetAutoScrollToEnd(DependencyObject obj, bool value) => 
        obj.SetValue(AutoScrollToEndProperty, value);

    private static void OnAutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox || listBox.Items.IsEmpty)
            return;

        var newValue = (bool) e.NewValue;
        if (newValue) 
            listBox.ScrollIntoView(listBox.Items[^1]);
    }
}