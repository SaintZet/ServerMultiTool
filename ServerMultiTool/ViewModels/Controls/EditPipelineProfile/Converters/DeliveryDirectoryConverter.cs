using System;
using System.Globalization;
using System.Windows.Data;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Converters;

public class DeliveryDirectoryConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return null;

        var source = values[0].ToString();
        var destination = values[1].ToString();

        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
            return null;

        return new DeliveryDirectoryWrapper(new DeliveryDirectories { Source = source, Destination = destination });
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}