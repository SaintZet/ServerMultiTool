using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters;

public class CollectionToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable enumerable && !(value is string))
        {
            var items = enumerable.Cast<object>().Select(x => x?.ToString() ?? "");
            return string.Join("; ", items);
        }

        return value?.ToString() ?? "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string stringValue)
            return value ?? "";

        var items = stringValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        // Return an ObservableCollection if that's what the target type is
        if (targetType == typeof(ObservableCollection<string>))
        {
            return new ObservableCollection<string>(items);
        }

        return items;
    }
}

