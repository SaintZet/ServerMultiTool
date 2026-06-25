using System;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters
{
    public class BoolToEnableDisableTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                return isEnabled ? "Disable" : "Enable";
            }
            return "Enable";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == "Disable";
        }
    }
}
