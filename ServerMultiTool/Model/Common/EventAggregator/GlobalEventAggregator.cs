using System;

namespace ServerMultiTool.Model.Common.EventAggregator;

public class GlobalEventAggregator : BaseEventAggregator
{
    private static readonly Lazy<GlobalEventAggregator> LazyInstance = new(() => new GlobalEventAggregator());

    public static GlobalEventAggregator Instance => LazyInstance.Value;

    private GlobalEventAggregator() { }

    public new void Publish<TEvent>(TEvent @event) =>
        base.Publish(@event);
}
