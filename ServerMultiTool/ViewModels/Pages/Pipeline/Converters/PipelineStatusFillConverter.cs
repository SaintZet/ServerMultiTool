using ServerMultiTool.ViewModels.Pages.Pipeline.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Converters
{
    public class PipelineStatusFillConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not PipelineOperationStatus milestoneIndicator)
                return DependencyProperty.UnsetValue;

            return milestoneIndicator switch
            {
                PipelineOperationStatus.Wait => GetClone("PrimaryGrayColor"),
                PipelineOperationStatus.InProgress => GetClone("PrimaryGreenColor"),
                PipelineOperationStatus.Success => GetClone("PrimaryGreenColor"),
                PipelineOperationStatus.Warning => GetClone("PrimaryYellowColor"),
                PipelineOperationStatus.Failure => GetClone("PrimaryRedColor"),
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