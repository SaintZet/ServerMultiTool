using System;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Converters
{
    public class TimestampConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime timestamp)
            {
                return timestamp.ToString("HH:mm:ss.fff");
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
