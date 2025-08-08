using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.MsBuild;

public partial class MsBuildOperationWrapper : ObservableObject, IPipelineOperationWrapper
{
    [ObservableProperty] bool _enabled;

    public string Name => "HTTP Ping";
    public string Description => "Pings a specified HTTP endpoint to check its availability.";

    readonly MsBuildOperation _operation;

    public MsBuildOperationWrapper(MsBuildOperation operation)
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
