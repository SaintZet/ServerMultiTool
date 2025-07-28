using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ServerMultiTool.Views.Behaviours;

public class ScrollingListBox : ListBox
{
    private ScrollViewer? _scrollViewer;
    private bool _autoScroll = true;
    private double _savedVerticalOffset = 0;

    public bool AutoScrollToEnd
    {
        get { return (bool)GetValue(AutoScrollToEndProperty); }
        set { SetValue(AutoScrollToEndProperty, value); }
    }

    public static readonly DependencyProperty AutoScrollToEndProperty =
        DependencyProperty.Register("AutoScrollToEnd", typeof(bool), typeof(ScrollingListBox),
                                    new PropertyMetadata(true));

    public ScrollingListBox()
    {
        this.Loaded += ScrollingListBox_Loaded;
        this.Unloaded += ScrollingListBox_Unloaded;
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (e.NewItems != null && e.NewItems.Count > 0 && _autoScroll && AutoScrollToEnd)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ScrollToEnd();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }

    private void ScrollingListBox_Loaded(object sender, RoutedEventArgs e)
    {
        _scrollViewer = FindVisualChild<ScrollViewer>(this);

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

            if (_savedVerticalOffset > 0)
            {
                _autoScroll = false;
                _scrollViewer.ScrollToVerticalOffset(_savedVerticalOffset);
                _autoScroll = IsScrollAtEnd();
            }
        }
    }

    private void ScrollingListBox_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_scrollViewer != null)
        {
            _savedVerticalOffset = _scrollViewer.VerticalOffset;
        }
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.ExtentHeightChange == 0 && e.ViewportHeightChange == 0)
        {
            _autoScroll = IsScrollAtEnd();
        }

        if (AutoScrollToEnd && _autoScroll && (e.ExtentHeightChange != 0 || e.ViewportHeightChange != 0))
        {
            ScrollToEnd();
        }
    }

    private bool IsScrollAtEnd()
    {
        if (_scrollViewer == null) return true;

        return Math.Abs(_scrollViewer.VerticalOffset - _scrollViewer.ScrollableHeight) < 1;
    }

    private void ScrollToEnd()
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollToEnd();
        }
    }

    private static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);

            if (child != null && child is T)
                return (T)child;

            T? childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }
}
