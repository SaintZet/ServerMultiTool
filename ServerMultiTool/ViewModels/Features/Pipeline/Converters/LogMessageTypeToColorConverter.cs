using ServerMultiTool.Model.Common.Logs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Converters
{
    public class LogMessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not LogMessageType messageType)
                return SystemColors.ControlTextBrush;

            return messageType switch
            {
                LogMessageType.Info => SystemColors.ControlTextBrush,
                LogMessageType.Warn => Brushes.Orange,
                LogMessageType.Error => Brushes.Red,
                LogMessageType.Exception => Brushes.Red,
                _ => SystemColors.ControlTextBrush
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}