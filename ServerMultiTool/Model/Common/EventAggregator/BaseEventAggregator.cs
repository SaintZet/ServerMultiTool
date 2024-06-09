using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.Common.EventAggregator;

public class BaseEventAggregator
{
    private readonly Dictionary<Type, List<Action<object>>> _eventHandlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);
        if (!_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType] = new List<Action<object>>();

        _eventHandlers[eventType].Add(evt => handler((TEvent)evt));
    }

    protected void Publish<TEvent>(TEvent @event)
    {
        if (@event is null)
            return;

        var eventType = typeof(TEvent);
        if (!_eventHandlers.ContainsKey(eventType))
            return;

        foreach (var handler in _eventHandlers[eventType])
            handler(@event);
    }
}