using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ServerMultiTool.ViewModels.Data;

namespace ServerMultiTool.ViewModels.Converters
{
    public class MilestoneFillConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MilestoneIndicator milestoneIndicator) 
                return DependencyProperty.UnsetValue;

            return milestoneIndicator switch
            {
                MilestoneIndicator.Wait => Application.Current.Resources["SecondaryGrayColor"] as Brush,
                MilestoneIndicator.Success => Application.Current.Resources["PrimaryGreenColor"] as Brush,
                MilestoneIndicator.Error => Application.Current.Resources["PrimaryRedColor"] as Brush,
                _ => DependencyProperty.UnsetValue
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}