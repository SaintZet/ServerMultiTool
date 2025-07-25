using ServerMultiTool.Model.Pipeline.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class HttpMonitoringService(HttpMonitoringSettings settings) : PipelineOperation
{
    protected override async Task<OperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        try
        {
            var urls = new List<string>();

            if (settings.PingSegment)
                urls.Add("http://localhost/Raid/Segment00/Segment.ashx");

            if (settings.PingMaster)
                urls.Add("http://localhost/Raid/Master00/Master.ashx");

            var timeout = settings.TimeoutMinutes;
            var tasks = urls.Select(url => MakeRequestAsync(url, timeout, cancellationToken)).ToList();

            var results = await Task.WhenAll(tasks);

            var resultDictionary = urls.Zip(results, (url, result) => new { url, result })
                .ToDictionary(item => item.url, item => item.result);

            var allComplete = resultDictionary.All(x => x.Value is OperationResult.Success);

            return allComplete ? OperationResult.Success : OperationResult.Failure;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled;
        }
    }

    private async Task<OperationResult> MakeRequestAsync(string url, double timeout, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Cancelled;

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(timeout);

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(url, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Logger.LogErrorWithPublish($"{url} request was canceled.");
            return OperationResult.Cancelled;
        }
        catch (Exception ex)
        {
            Logger.LogErrorWithPublish($"{url} exception: {ex.Message}");
            return OperationResult.Failure;
        }

        var message = $"{url} return {response.StatusCode}";

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            Logger.LogInfoWithPublish(message);
            return OperationResult.Success;
        }

        Logger.LogErrorWithPublish(message);
        return OperationResult.Failure;
    }
}