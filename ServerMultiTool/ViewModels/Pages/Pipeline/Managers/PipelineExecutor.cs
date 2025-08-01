using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.Pipeline.Contracts;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Managers;

public class PipelineExecutor
{
    private readonly Logger _logger;
    private readonly GsLogMonitor _logManager;

    private CancellationTokenSource? _pipelineCancellationTokenSource;

    public PipelineStepsCollection PipelineSteps { get; private set; } = [];

    public event EventHandler<bool>? PipelineStateChanged;

    public PipelineExecutor(GsLogMonitor logManager)
    {
        _logger = new Logger(GetType());
        _logManager = logManager;
    }

    public void UpdateOperations(PipelineProfileWrapper pipelineProfile)
    {
        PipelineSteps = pipelineProfile.Steps;
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
        PipelineSteps.ClearStatuses();

        _pipelineCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _pipelineCancellationTokenSource.Token;

        await ExecuteSequentially(generalInfo, cancellationToken);
    }

    private async Task ExecuteSequentially(GeneralInfoViewModel generalInfo, CancellationToken cancellationToken)
    {
        foreach (var step in PipelineSteps)
        {
            if (ShouldCancelExecution(cancellationToken))
                break;

            PrepareOperation(generalInfo, step);

            if (!await ExecuteOperationSafely(step, cancellationToken))
                break;
        }
    }

    private bool ShouldCancelExecution(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            return false;

        PipelineSteps.CancelWaiting();
        return true;
    }

    private static void PrepareOperation(GeneralInfoViewModel generalInfo, PipelineStepWrapper step)
    {
        step.Started();

        if (generalInfo.SelectedSolutionDirectory is not null) // todo: check if this is necessary
            step.UpdateSolutionDirectory(generalInfo.SelectedSolutionDirectory);

        if (generalInfo.SelectedHttpDirectory is not null)
            step.UpdateHttpDirectory(generalInfo.SelectedHttpDirectory);
    }

    private async Task<bool> ExecuteOperationSafely(PipelineStepWrapper step, CancellationToken cancellationToken)
    {
        try
        {
            var result = await step.ExecuteAsync(cancellationToken);

            if (result == PipelineOperationResult.Cancelled)
            {
                PipelineSteps.CancelWaiting();
                return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            step.Cancel();
            PipelineSteps.CancelWaiting();
            return false;
        }
    }

    private void CompletePipelineExecution(GeneralInfoViewModel generalInfo)
    {
        PipelineStateChanged?.Invoke(this, false);
        generalInfo.CanChangeStates = true;
    }
}