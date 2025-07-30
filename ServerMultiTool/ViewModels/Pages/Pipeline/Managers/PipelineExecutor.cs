using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Pipeline.Contracts;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using ServerMultiTool.ViewModels.Pages.Pipeline.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Managers;

public class PipelineExecutor
{
    private readonly Logger _logger;
    private readonly LogMonitoringManager _logManager;
    private CancellationTokenSource? _pipelineCancellationTokenSource;

    public PipelineOperationCollection PipelineOperations { get; private set; } = [];

    public event EventHandler<bool>? PipelineStateChanged;

    public PipelineExecutor(LogMonitoringManager logManager)
    {
        _logger = new Logger(GetType());
        _logManager = logManager;
    }

    public void UpdateOperations(PipelineProfile pipelineProfile)
    {
        var operations = PipelineOperationFactory.CreatePipelineOperations(pipelineProfile);
        PipelineOperations = operations;
    }

    public void StopPipeline()
    {
        _logManager.ClearAppLogs();
        _pipelineCancellationTokenSource?.Cancel();
    }

    public async Task ExecutePipeline(GeneralInfoViewModel generalInfo)
    {
        try
        {
            await StartPipelineExecution(generalInfo);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInfoWithPublish("Pipeline execution was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogErrorWithPublish($"Error during pipeline execution: {ex.Message}");
        }
        finally
        {
            CompletePipelineExecution(generalInfo);
        }
    }

    private async Task StartPipelineExecution(GeneralInfoViewModel generalInfo)
    {
        PipelineStateChanged?.Invoke(this, true);
        generalInfo.CanChangeStates = false;

        _logManager.ClearAppLogs();
        PipelineOperations.ClearStatuses();

        _pipelineCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _pipelineCancellationTokenSource.Token;

        await ExecuteOperationsSequentially(generalInfo, cancellationToken);
    }

    private async Task ExecuteOperationsSequentially(GeneralInfoViewModel generalInfo, CancellationToken cancellationToken)
    {
        foreach (var operation in PipelineOperations)
        {
            if (ShouldCancelExecution(cancellationToken))
                break;

            PrepareOperation(generalInfo, operation);

            if (!await ExecuteOperationSafely(operation, cancellationToken))
                break;
        }
    }

    private bool ShouldCancelExecution(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            return false;

        PipelineOperations.CancelWaitingOperations();
        return true;
    }

    private static void PrepareOperation(GeneralInfoViewModel generalInfo, PipelineOperationWrapper operation)
    {
        operation.OperationStarted();

        if (generalInfo.SelectedSolutionDirectory is not null)
            operation.UpdateSolutionDirectory(generalInfo.SelectedSolutionDirectory);

        if (generalInfo.SelectedHttpDirectory is not null)
            operation.UpdateHttpDirectory(generalInfo.SelectedHttpDirectory);
    }

    private async Task<bool> ExecuteOperationSafely(PipelineOperationWrapper operation, CancellationToken cancellationToken)
    {
        try
        {
            var result = await operation.ExecuteAsync(cancellationToken);

            if (result == OperationResult.Cancelled)
            {
                PipelineOperations.CancelWaitingOperations();
                return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            operation.CancelOperation();
            PipelineOperations.CancelWaitingOperations();
            return false;
        }
    }

    private void CompletePipelineExecution(GeneralInfoViewModel generalInfo)
    {
        PipelineStateChanged?.Invoke(this, false);
        generalInfo.CanChangeStates = true;
    }
}