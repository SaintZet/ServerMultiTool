using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ServerMultiTool.ViewModels.Features.Pipeline.Enums;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Converters
{
    public class PipelineStatusFillConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not PipelineStepStatus milestoneIndicator)
                return DependencyProperty.UnsetValue;

            return milestoneIndicator switch
            {
                PipelineStepStatus.Wait => GetClone("PrimaryGrayColor"),
                PipelineStepStatus.InProgress => GetClone("PrimaryGreenColor"),
                PipelineStepStatus.Success => GetClone("PrimaryGreenColor"),
                PipelineStepStatus.Warning => GetClone("PrimaryYellowColor"),
                PipelineStepStatus.Failure => GetClone("PrimaryRedColor"),
                _ => DependencyProperty.UnsetValue
            };
        }

        private static SolidColorBrush? GetClone(string key)
        {
            if (Application.Current.Resources[key] is SolidColorBrush brush)
                return brush.Clone();

            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
