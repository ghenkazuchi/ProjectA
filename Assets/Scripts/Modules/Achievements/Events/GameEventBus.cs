using System;
using System.Collections.Generic;

public static class GameEventBus
{
    private static readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

    public static void Subscribe<T>(Action<T> action) where T : struct, IGameEvent
    {
        Type t = typeof(T);
        if (!subscribers.ContainsKey(t))
        {
            subscribers[t] = new List<Delegate>();
        }
        subscribers[t].Add(action);
    }

    public static void Unsubscribe<T>(Action<T> action) where T : struct, IGameEvent
    {
        Type t = typeof(T);
        if (subscribers.ContainsKey(t))
        {
            subscribers[t].Remove(action);
        }
    }

    public static void Publish<T>(T ev) where T : struct, IGameEvent
    {
        Type t = typeof(T);
        if (subscribers.ContainsKey(t))
        {
            var actions = subscribers[t];
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] is Action<T> action)
                {
                    action.Invoke(ev);
                }
            }
        }
        
        // Also fire the generic catch-all for observers that want any IGameEvent (like AchievementService)
        Type catchAll = typeof(IGameEvent);
        if (subscribers.ContainsKey(catchAll))
        {
            var actions = subscribers[catchAll];
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] is Action<IGameEvent> action)
                {
                    action.Invoke(ev);
                }
            }
        }
    }
    
    public static void SubscribeToAll(Action<IGameEvent> action)
    {
        Type t = typeof(IGameEvent);
        if (!subscribers.ContainsKey(t))
        {
            subscribers[t] = new List<Delegate>();
        }
        subscribers[t].Add(action);
    }
    
    public static void UnsubscribeToAll(Action<IGameEvent> action)
    {
        Type t = typeof(IGameEvent);
        if (subscribers.ContainsKey(t))
        {
            subscribers[t].Remove(action);
        }
    }
}
