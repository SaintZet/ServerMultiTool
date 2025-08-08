using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Delivery;

public partial class DeliveryBinOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    public string Name => "default name";
    public string Description => "default description";

    readonly DeliveryBinOperation _operation;

    public DeliveryBinOperationWrapper(DeliveryBinOperation operation)
    {
        _operation = operation;

        Enabled = operation.Enabled;
    }

    public async Task<PipelineOperationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _operation.ExecuteAsync(cancellationToken);
    }

    public PipelineOperation ToOriginal()
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
