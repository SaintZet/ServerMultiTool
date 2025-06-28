using System;
using System.Globalization;
using System.Windows.Data;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Wrappers;

namespace ServerMultiTool.ViewModels.Controls.EditPipelineProfile.Converters;

public class ProcessEventConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return null;

        var path = values[0].ToString();
        var arguments = values[1].ToString();

        if (string.IsNullOrEmpty(path))
            return null;

        return new ProcessEventWrapper(new ProcessEvent { Path = path, Arguments = arguments });
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}