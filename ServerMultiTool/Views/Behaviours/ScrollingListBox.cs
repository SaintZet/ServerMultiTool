using System;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace ServerMultiTool.Views.Behaviours;

public class ScrollingListBox : ListBox
{
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            var newItemCount = e.NewItems.Count; 

            if(newItemCount > 0) 
                ScrollIntoView(e.NewItems[newItemCount - 1] ?? throw new InvalidOperationException());
        }

        base.OnItemsChanged(e);
    } 
}
