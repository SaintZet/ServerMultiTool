using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ServerMultiTool.ViewModels.Converters
{
    public class OperationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DeliveryBinOperationTemplate { get; set; } = null!;
        public DataTemplate DeliverySpecifiedFilesOperationTemplate { get; set; } = null!;
        public DataTemplate HttpPingOperationTemplate { get; set; } = null!;
        public DataTemplate ProcessExecutionOperationTemplate { get; set; } = null!;
        public DataTemplate SqlExecutionOperationTemplate { get; set; } = null!;
        public DataTemplate WebBrowserOperationTemplate { get; set; } = null!;
        public DataTemplate MsBuildOperationTemplate { get; set; } = null!;
        public DataTemplate GitPullOperationTemplate { get; set; } = null!;


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is not PipelineOperationWrapper pipelineOperationWrapper)
                throw new NotImplementedException(); // todo: add a more descriptive exception message

            switch (pipelineOperationWrapper.Operation.OperationType)
            {
                case OperationType.DeliveryBinOperation:
                    return DeliveryBinOperationTemplate;

                case OperationType.DeliverySpecifiedFilesOperation:
                    return DeliverySpecifiedFilesOperationTemplate;

                case OperationType.HttpPingOperation:
                    return HttpPingOperationTemplate;

                case OperationType.ProcessExecutionOperation:
                    return ProcessExecutionOperationTemplate;

                case OperationType.SqlExecutionOperation:
                    return SqlExecutionOperationTemplate;

                case OperationType.WebBrowserOperation:
                    return WebBrowserOperationTemplate;

                case OperationType.MsBuildOperation:
                    return MsBuildOperationTemplate;

                case OperationType.GitPullOperation:
                    return GitPullOperationTemplate;

                default:
                    throw new NotImplementedException(); // todo: add a more descriptive exception message
            }
        }
    }
}
