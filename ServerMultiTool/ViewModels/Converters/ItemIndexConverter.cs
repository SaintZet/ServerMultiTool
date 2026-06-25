using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters;

/// <summary>
/// Converter that finds the index of an item in a collection and adds 1 (for 1-based numbering).
/// Used with MultiBinding: first binding is the item, second binding is the collection.
/// </summary>
public class ItemIndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[1] is IList collection && values[0] != null)
        {
            int index = collection.IndexOf(values[0]);
            return index >= 0 ? index + 1 : 1;
        }
        return 1;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}


