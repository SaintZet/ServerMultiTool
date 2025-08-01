using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers;
using System;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers
{
    public class PipelineOperationWrapperFactory
    {
        public static IPipelineOperationWrapper Create(IPipelineOperation operation)
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