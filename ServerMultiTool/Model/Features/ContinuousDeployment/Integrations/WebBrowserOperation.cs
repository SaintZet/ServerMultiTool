using ServerMultiTool.Model.Domain.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Features.ContinuousDeployment.Integrations
{
    public class WebBrowserOperation(string name) : PipelineOperation(name)
    {
        public override OperationType OperationType => OperationType.WebBrowserOperation;

        [JsonInclude] public List<string> Urls { get; private set; } = [];

        public WebBrowserOperation UpdateUrls(List<string> urls)
        {
            if (urls == null || urls.Count == 0)
                throw new ArgumentException("URLs cannot be null or empty.", nameof(urls));

            Urls = urls;
            return this;
        }

        public WebBrowserOperation AddUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));

            Urls.Add(url);
            return this;
        }

        public WebBrowserOperation RemoveUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));

            Urls.Remove(url);
            return this;
        }

        protected override async Task<PipelineOperationResult> ExecuteOperationsAsync(CancellationToken cancellationToken)
        {
            var tasks = Urls.Select(async url => await OpenPageAsync(url, cancellationToken));

            var results = await Task.WhenAll(tasks.Select(task => task.ContinueWith(t => PipelineOperationResult.Success, cancellationToken)));

            var resultDictionary = Urls.Zip(results, (url, result) => new { url, result })
                .ToDictionary(item => item.url, item => item.result);

            var allComplete = resultDictionary.All(x => x.Value == PipelineOperationResult.Success);

            return allComplete ? PipelineOperationResult.Success : PipelineOperationResult.Failure;
        }

        private async Task OpenPageAsync(string url, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });
            }, cancellationToken);

            Logger.LogInfoWithPublish("The web page has been successfully opened.");
        }
    }
}
