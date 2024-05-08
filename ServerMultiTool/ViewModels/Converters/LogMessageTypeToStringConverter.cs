using System;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters;

public class LogMessageTypeToStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
            return enumValue.ToString();
        
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}