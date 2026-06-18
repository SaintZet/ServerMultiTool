using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Network;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.Integrations;

public partial class WebBrowserOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "Web Browser Operation";

    [ObservableProperty] ObservableCollection<string> _urls = [];

    public WebBrowserOperationWrapper(WebBrowserOperation operation) : base(operation)
    {
        Urls = new ObservableCollection<string>(operation.Urls);
        Urls.CollectionChanged += Urls_CollectionChanged;
    }

    private void Urls_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var op = (WebBrowserOperation)Operation;
        op.UpdateUrls(Urls.ToList());
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

        var op = (WebBrowserOperation)Operation;
        op.UpdateUrls([.. Urls]);
        return op;
    }
}