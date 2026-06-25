using System;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters;

public class BoolToEnabledDisabledTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isEnabled && isEnabled
            ? "Enabled"
            : "Disable";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.Equals(value?.ToString(), "Enabled", StringComparison.OrdinalIgnoreCase);
    }
}

