using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.Features.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.Features.ContinuousIntegration.Git;
using ServerMultiTool.Model.Features.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Delivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Git;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.MsBuild;
using System;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base
{
    public class PipelineOperationWrapperFactory
    {
        public static IPipelineOperationWrapper Create(PipelineOperation operation)
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