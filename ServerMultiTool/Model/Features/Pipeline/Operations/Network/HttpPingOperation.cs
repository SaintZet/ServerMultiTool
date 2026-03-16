using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.Pipeline.Operations.Network;

public class HttpPingOperation : PipelineOperationBase
{
    public override OperationType OperationType => OperationType.HttpPingOperation;

    [JsonInclude] public List<string> Urls { get; private set; } = [];
    [JsonInclude] public double TimeoutInMinutes { get; set; } = 5;

    public HttpPingOperation(string name)
            : base(name)
    {

    }

    public HttpPingOperation UpdateUrls(List<string> urls)
    {
        Urls = urls;
        return this;
    }

    public HttpPingOperation AddUrl(string url)
    {
        Urls.Add(url);
        return this;
    }

    public HttpPingOperation RemoveUrl(string url)
    {
        Urls.Remove(url);
        return this;
    }

    public HttpPingOperation UpdateTimeoutInMinutes(double timeoutInMinutes)
    {
        if (timeoutInMinutes <= 0)
            throw new ArgumentException("Timeout must be greater than zero.", nameof(timeoutInMinutes));

        TimeoutInMinutes = timeoutInMinutes;
        return this;
    }

    protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
    {
        var tasks = Urls.Select(async url => await MakeRequestAsync(url, TimeoutInMinutes, cancellationToken));

        var results = await Task.WhenAll(tasks.Select(task => task.ContinueWith(t => PipelineOperationResult.Success, cancellationToken)));

        var resultDictionary = Urls.Zip(results, (url, result) => new { url, result })
            .ToDictionary(item => item.url, item => item.result);

        var allComplete = resultDictionary.All(x => x.Value == PipelineOperationResult.Success);

        return allComplete ? PipelineOperationResult.Success : PipelineOperationResult.Failure;
    }

    private async Task<PipelineOperationResult> MakeRequestAsync(string url, double timeout, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return PipelineOperationResult.Cancelled;

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
            return PipelineOperationResult.Cancelled;
        }
        catch (Exception ex)
        {
            Logger.LogErrorWithPublish($"{url} exception: {ex.Message}");
            return PipelineOperationResult.Failure;
        }

        var message = $"{url} return {response.StatusCode}";

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            Logger.LogSuccessWithPublish(message);
            return PipelineOperationResult.Success;
        }

        Logger.LogErrorWithPublish(message);
        return PipelineOperationResult.Failure;
    }
}