using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Network;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class HttpPingOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "HTTP Ping";

    [ObservableProperty] double _timeoutInMinutes;
    [ObservableProperty] ObservableCollection<string> _urls = [];

    public HttpPingOperationWrapper(HttpPingOperation operation) : base(operation)
    {
        Urls = new ObservableCollection<string>(operation.Urls);
        Urls.CollectionChanged += Urls_CollectionChanged;
        TimeoutInMinutes = operation.TimeoutInMinutes;
    }

    private void Urls_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var op = (HttpPingOperation)Operation;
        op.UpdateUrls([.. Urls]);
    }

    partial void OnTimeoutInMinutesChanged(double value)
    {
        if (value > 0)
        {
            var op = (HttpPingOperation)Operation;
            op.UpdateTimeoutInMinutes(value);
        }
    }

    [RelayCommand]
    private void AddUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        var trimmed = url.Trim();
        if (!Urls.Contains(trimmed))
            Urls.Add(trimmed);
    }

    [RelayCommand]
    private void RemoveUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        Urls.Remove(url);
    }

    public override PipelineOperationBase ToOriginal()
    {
        base.ToOriginal();

        var op = (HttpPingOperation)Operation;
        op.UpdateUrls([.. Urls]);
        op.UpdateTimeoutInMinutes(TimeoutInMinutes);
        return op;
    }
}
