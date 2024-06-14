using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ServerMultiTool.Model.Pipeline.Contracts;

namespace ServerMultiTool.Model.ContinuousDeployment.Integrations;

public class HttpMonitoringService : PipelineOperation
{
    private readonly HttpMonitoringSettings _settings;

    public HttpMonitoringService(HttpMonitoringSettings settings) => 
        _settings = settings;

    protected override async Task<OperationResult> ExecuteOperationsAsync()
    {
        var urls = new List<string>();

        if (_settings.PingSegment) 
            urls.Add("http://localhost/Raid/Segment00/Segment.ashx");

        if (_settings.PingMaster) 
            urls.Add("http://localhost/Raid/Master00/Master.ashx");

        var timeout = _settings.TimeoutMinutes;
        var tasks = urls.Select(url => MakeRequestAsync(url, timeout)).ToList();

        var results = await Task.WhenAll(tasks);

        var resultDictionary = urls.Zip(results, (url, result) => new { url, result })
            .ToDictionary(item => item.url, item => item.result);

        var allComplete = resultDictionary.All(x => x.Value is OperationResult.Success);

        return allComplete ? OperationResult.Success : OperationResult.Failure;
    }

    private async Task<OperationResult> MakeRequestAsync(string url, double timeout)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(timeout) };
            
        var response = await client.GetAsync(url);
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