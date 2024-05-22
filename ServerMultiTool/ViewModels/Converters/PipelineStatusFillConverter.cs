using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ServerMultiTool.ViewModels.Data;

namespace ServerMultiTool.ViewModels.Converters
{
    public class PipelineStatusFillConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not PipelineOperationStatus milestoneIndicator) 
                return DependencyProperty.UnsetValue;

            return milestoneIndicator switch
            {
                PipelineOperationStatus.Wait => Application.Current.Resources["SecondaryGrayColor"] as Brush,
                PipelineOperationStatus.Success => Application.Current.Resources["PrimaryGreenColor"] as Brush,
                PipelineOperationStatus.Warning => Application.Current.Resources["PrimaryYellowColor"] as Brush,
                PipelineOperationStatus.Failure => Application.Current.Resources["PrimaryRedColor"] as Brush,
                _ => DependencyProperty.UnsetValue
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}