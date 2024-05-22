using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.Common.EventAggregator;

public static class GlobalEventAggregator
{
    private static readonly Dictionary<Type, List<Action<object>>> EventHandlers = new();
    
    public static void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);
        if (!EventHandlers.ContainsKey(eventType))
            EventHandlers[eventType] = new List<Action<object>>();

        EventHandlers[eventType].Add(evt => handler((TEvent)evt));
    }

    public static void Publish<TEvent>(TEvent @event)
    {
        if (@event is null)
            return;
        
        var eventType = typeof(TEvent);
        if (!EventHandlers.ContainsKey(eventType))
            return;

        foreach (var handler in EventHandlers[eventType])
            handler(@event);
    }
}