using System;
using System.Windows;
using System.Windows.Controls;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Converters;

public class OperationEditorTemplateSelector : DataTemplateSelector
{
    public DataTemplate? GitPullTemplate { get; set; }
    public DataTemplate? MsBuildTemplate { get; set; }
    public DataTemplate? DeliveryBinTemplate { get; set; }
    public DataTemplate? DeliverySpecifiedFilesTemplate { get; set; }
    public DataTemplate? HttpPingTemplate { get; set; }
    public DataTemplate? ProcessExecutionTemplate { get; set; }
    public DataTemplate? SqlExecutionTemplate { get; set; }
    public DataTemplate? WebBrowserTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not PipelineOperationWrapper wrapper)
            return null;

        return wrapper.Operation.OperationType switch
        {
            OperationType.GitPullOperation => GitPullTemplate,
            OperationType.MsBuildOperation => MsBuildTemplate,
            OperationType.DeliveryBinOperation => DeliveryBinTemplate,
            OperationType.DeliverySpecifiedFilesOperation => DeliverySpecifiedFilesTemplate,
            OperationType.HttpPingOperation => HttpPingTemplate,
            OperationType.ProcessExecutionOperation => ProcessExecutionTemplate,
            OperationType.SqlExecutionOperation => SqlExecutionTemplate,
            OperationType.WebBrowserOperation => WebBrowserTemplate,
            _ => null
        };
    }
}

