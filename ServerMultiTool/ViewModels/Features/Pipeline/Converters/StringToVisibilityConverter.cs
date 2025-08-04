using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.IdentityModel.Tokens;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string stringValue) 
            return Visibility.Collapsed;
        
        return stringValue.IsNullOrEmpty() ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}