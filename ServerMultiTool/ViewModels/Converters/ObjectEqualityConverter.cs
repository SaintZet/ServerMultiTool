using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters;

/// <summary>
/// Converts multiple values by checking if they are equal (using Equals method).
/// Used primarily for DataTrigger and MultiDataTrigger Value comparisons.
/// </summary>
public class ObjectEqualityConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return false;

        var first = values[0];
        var second = values[1];

        return Equals(first, second);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ObjectEqualityConverter does not support ConvertBack");
    }
}

