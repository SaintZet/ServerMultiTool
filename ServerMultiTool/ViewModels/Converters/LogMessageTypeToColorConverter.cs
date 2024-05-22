using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ServerMultiTool.Model.Common.Logs;

namespace ServerMultiTool.ViewModels.Converters
{
    public class LogMessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LogMessageType messageType)
                return Brushes.Black;

            return messageType switch
            {
                LogMessageType.Info => Brushes.Black,
                LogMessageType.Warn => Brushes.Orange,
                LogMessageType.Error => Brushes.Red,
                _ => Brushes.Black
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}