using System;
using System.Globalization;
using System.Windows.Data;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;

namespace ServerMultiTool.ViewModels.Converters;

public class OperationTypeToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not OperationType opType) return "?";
        return opType switch
        {
            OperationType.GitPullOperation => "GIT",
            OperationType.MsBuildOperation => "MSB",
            OperationType.DeliveryBinOperation => "BIN",
            OperationType.DeliverySpecifiedFilesOperation => "DEL",
            OperationType.HttpPingOperation => "HTTP",
            OperationType.ProcessExecutionOperation => "EXE",
            OperationType.SqlExecutionOperation => "SQL",
            OperationType.WebBrowserOperation => "WEB",
            _ => "?"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

