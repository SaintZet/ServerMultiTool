using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b)
            return Visibility.Collapsed;

        var invert = parameter?.ToString()?.ToLower() is "invert";
        if (invert)
            b = !b;

        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Visibility v)
            return false;

        var result = v is Visibility.Visible;

        var invert = parameter?.ToString()?.ToLower() is "invert";

        return invert ? !result : result;
    }
}
