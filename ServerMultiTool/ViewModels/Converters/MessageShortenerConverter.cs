using System;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters;

public class MessageShortenerConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string stringValue || !stringValue.Contains("\n")) 
            return value;
        
        var lines = stringValue.Split('\n');
        return lines[0];

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}