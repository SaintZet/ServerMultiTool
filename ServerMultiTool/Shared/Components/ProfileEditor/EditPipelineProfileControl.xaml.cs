using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ServerMultiTool.Shared.Components.ProfileEditor;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;

namespace ServerMultiTool.Shared.Components.ProfileEditor;

public partial class EditPipelineProfileControl : UserControl
{
    private Point _startPoint;
    private bool _isDragging;
    private PipelineStepWrapper? _draggedItem;

    public EditPipelineProfileControl()
    {
        InitializeComponent();
    }

    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
        _isDragging = false;
    }

    private void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        var mousePos = e.GetPosition(null);
        var diff = _startPoint - mousePos;

        if (e.LeftButton == MouseButtonState.Pressed &&
            !_isDragging &&
            (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
             Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            var listViewItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            _draggedItem = (PipelineStepWrapper)listViewItem.DataContext;
            if (_draggedItem == null)
                return;

            _isDragging = true;
            var dragData = new DataObject("PipelineStepWrapper", _draggedItem);
            DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
        }
    }

    private void ListView_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("PipelineStepWrapper") || e.Source == sender)
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        e.Effects = DragDropEffects.Move;
    }

    private void ListView_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("PipelineStepWrapper"))
            return;

        if (sender is not ListView listView)
            return;

        var droppedItem = e.Data.GetData("PipelineStepWrapper") as PipelineStepWrapper;

        var dropPosition = e.GetPosition(listView);
        var targetItem = GetItemAt(listView, dropPosition);

        if (targetItem == null)
            return;

        if (targetItem.DataContext is not PipelineStepWrapper targetStep || targetStep == droppedItem)
            return;

        var viewModel = DataContext as EditPipelineProfileViewModel;
        viewModel?.ReorderSteps(droppedItem, targetStep);
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T ancestor)
                return ancestor;
            current = VisualTreeHelper.GetParent(current);
        }
        while (current != null);

        return null;
    }

    private static ListViewItem? GetItemAt(ListView listView, Point position)
    {
        var element = listView.InputHitTest(position) as DependencyObject;

        while (element != null && element is not ListViewItem)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        return element as ListViewItem;
    }
}

