using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Common;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.Pipeline.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers.OperationsWrappers
{
    public partial class DeliverySpecifiedFilesOperationWrapper : ObservableObject, IPipelineOperationWrapper
    {
        [ObservableProperty] bool _enabled;

        public string Name => "default name";
        public string Description => "default description";

        readonly DeliverySpecifiedFilesOperation _operation;

        public DeliverySpecifiedFilesOperationWrapper(DeliverySpecifiedFilesOperation operation)
        {
            _operation = operation;

            Enabled = operation.Enabled;
        }

        public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await _operation.ExecuteAsync(cancellationToken);
        }

        public IPipelineOperation ToOriginal()
        {
            _operation.EnableOperation(Enabled);

            return _operation;
        }

        public void UpdateSolutionDirectory(DirectoryModel directory)
        {
            _operation.UpdateSolutionDirectory(directory);
        }

        public void UpdateHttpDirectory(DirectoryModel directory)
        {
            _operation.UpdateHttpDirectory(directory);
        }
    }
}