using System;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.Model.Features.Pipeline.Operations.FileDelivery;
using ServerMultiTool.Model.Features.Pipeline.Operations.Network;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineOperations.Execution;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineOperations.FileDelivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineOperations.Network;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base
{
    public class PipelineOperationWrapperFactory
    {
        public static PipelineOperationWrapper Create(PipelineOperationBase operation)
        {
            if (operation is null)
                throw new ArgumentNullException(nameof(operation), "Operation cannot be null.");

            return operation switch
            {
                GitPullOperation gitPullOperation => new GitPullOperationWrapper(gitPullOperation),
                MsBuildOperation msBuildOperation => new MsBuildOperationWrapper(msBuildOperation),
                DeliveryBinOperation deliveryBinOperation => new DeliveryBinOperationWrapper(deliveryBinOperation),
                DeliverySpecifiedFilesOperation deliverySpecifiedFilesOperation => new DeliverySpecifiedFilesOperationWrapper(deliverySpecifiedFilesOperation),
                HttpPingOperation httpPingOperation => new HttpPingOperationWrapper(httpPingOperation),
                ProcessExecutionOperation iisResetOperation => new ProcessExecutionOperationWrapper(iisResetOperation),
                SqlExecutionOperation sqlExecutionOperation => new SqlExecutionOperationWrapper(sqlExecutionOperation),
                WebBrowserOperation webBrowserOperation => new WebBrowserOperationWrapper(webBrowserOperation),
                _ => throw new ArgumentNullException(nameof(operation), "Can`t find operation."),
            };
        }
    }
}
