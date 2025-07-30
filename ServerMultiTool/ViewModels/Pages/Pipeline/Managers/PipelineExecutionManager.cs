using ServerMultiTool.Model.Common.Logs;
using ServerMultiTool.Model.ContinuousDeployment.Delivery;
using ServerMultiTool.Model.ContinuousDeployment.Integrations;
using ServerMultiTool.Model.ContinuousIntegration.Git;
using ServerMultiTool.Model.ContinuousIntegration.MsBuild;
using ServerMultiTool.Model.Pipeline.Contracts;
using ServerMultiTool.Model.Pipeline.Profiles;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages.Pipeline.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Managers;

public class PipelineExecutionManager
{
    private readonly Logger _logger;
    private readonly LogMonitoringManager _logManager;

    private CancellationTokenSource? _pipelineCancellationTokenSource;

    public PipelineOperationCollection PipelineOperations { get; } = [];

    public event EventHandler<bool>? PipelineStateChanged;

    public PipelineExecutionManager(LogMonitoringManager logManager)
    {
        _logger = new Logger(GetType());
        _logManager = logManager;
    }

    public void UpdatePipelineOperations(PipelineProfile pipeline)
    {
        PipelineOperations.Clear();

        if (pipeline.GitSettings.Enable)
            PipelineOperations.Add(new(new GitService(pipeline.GitSettings), "Git"));

        if (pipeline.SettingsPerProject.Any(x => x.MsBuildSettings.Enable))
            PipelineOperations.Add(new(new MsBuildService(pipeline.SettingsPerProject), "MsBuild"));

        if (pipeline.InternetInformationSettings.Enable)
            PipelineOperations.Add(new(new InternetInformationServices("/stop", pipeline.InternetInformationSettings), "IIS Stop"));

        if (pipeline.SettingsPerProject.Any(x => x.DeliverySettings.EnableCustomDelivery || x.DeliverySettings.EnableDeliveryBin))
            PipelineOperations.Add(new(new DeliveryService(pipeline.SettingsPerProject), "Delivery"));

        if (pipeline.SqlExecutionSettings.Enable)
            PipelineOperations.Add(new(new SqlExecutionService(pipeline.SqlExecutionSettings), "SQL"));

        if (pipeline.InternetInformationSettings.Enable)
            PipelineOperations.Add(new(new InternetInformationServices("/start", pipeline.InternetInformationSettings), "IIS Start"));

        if (pipeline.WebBrowserSettings.Enable)
            PipelineOperations.Add(new(new WebBrowserService(pipeline.WebBrowserSettings), "Web"));

        if (pipeline.HttpMonitoringSettings.Enable)
            PipelineOperations.Add(new(new HttpMonitoringService(pipeline.HttpMonitoringSettings), "Http"));
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
            if (cancellationToken.IsCancellationRequested)
            {
                PipelineOperations.CancelWaitingOperations();
                break;
            }

            operation.OperationStarted();

            if (generalInfo.SelectedSolutionDirectory is not null)
                operation.UpdateSolutionDirectory(generalInfo.SelectedSolutionDirectory);

            if (generalInfo.SelectedHttpDirectory is not null)
                operation.UpdateHttpDirectory(generalInfo.SelectedHttpDirectory);

            try
            {
                var result = await operation.ExecuteAsync(cancellationToken);

                if (result == OperationResult.Cancelled)
                {
                    PipelineOperations.CancelWaitingOperations();
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                operation.CancelOperation();

                PipelineOperations.CancelWaitingOperations();
                break;
            }
        }
    }

    private void CompletePipelineExecution(GeneralInfoViewModel generalInfo)
    {
        PipelineStateChanged?.Invoke(this, false);
        generalInfo.CanChangeStates = true;
    }
}