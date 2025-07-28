using System;
using System.Globalization;
using System.Windows.Data;

namespace ServerMultiTool.ViewModels.Converters
{
    public class NumericValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                double multiplier = 1.0;
                if (parameter is string paramStr && double.TryParse(paramStr, out double paramValue))
                {
                    multiplier = paramValue;
                }
                else if (parameter is double paramDouble)
                {
                    multiplier = paramDouble;
                }

                return doubleValue * multiplier;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
