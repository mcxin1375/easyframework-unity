/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/6
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public interface IEvent<T>
    {
        void Execute(in T args);
    }

    internal static class Event<T>
    {
        private static readonly List<IEvent<T>> EventList = new();
        private static readonly List<Action<T>> ActionList = new();
        private static readonly List<IEvent<T>> EventSnapshot = new();
        private static readonly List<Action<T>> ActionSnapshot = new();

        public static void Add(IEvent<T> handler)
        {
            if (handler == null) return;
            if (!EventList.Contains(handler)) EventList.Add(handler);
        }

        public static void Remove(IEvent<T> handler)
        {
            if (handler == null) return;
            EventList.Remove(handler);
        }

        public static void Add(Action<T> handler)
        {
            if (handler == null) return;
            if (!ActionList.Contains(handler)) ActionList.Add(handler);
        }

        public static void Remove(Action<T> handler)
        {
            if (handler == null) return;
            ActionList.Remove(handler);
        }

        public static void Invoke(in T args)
        {
            int eventStart = EventSnapshot.Count;
            int actionStart = ActionSnapshot.Count;
            try
            {
                int eventCount = EventList.Count;
                int actionCount = ActionList.Count;
                // Capture both groups before callbacks; nested dispatch appends its own range.
                EventSnapshot.AddRange(EventList);
                ActionSnapshot.AddRange(ActionList);

                for (int i = 0; i < eventCount; i++)
                {
                    try
                    {
                        EventSnapshot[eventStart + i].Execute(args);
                    }
                    catch (Exception e)
                    {
                        FDebug.LogError(e);
                    }
                }

                for (int i = 0; i < actionCount; i++)
                {
                    try
                    {
                        ActionSnapshot[actionStart + i]?.Invoke(args);
                    }
                    catch (Exception e)
                    {
                        FDebug.LogError(e);
                    }
                }
            }
            finally
            {
                EventSnapshot.RemoveRange(eventStart, EventSnapshot.Count - eventStart);
                ActionSnapshot.RemoveRange(actionStart, ActionSnapshot.Count - actionStart);
            }
        }
    }

    public class Event : Singleton<Event>
    {
        public void Add<T>(IEvent<T> instance) => Event<T>.Add(instance);
        public void Remove<T>(IEvent<T> instance) => Event<T>.Remove(instance);

        public void Add<T>(Action<T> handler) => Event<T>.Add(handler);
        public void Remove<T>(Action<T> handler) => Event<T>.Remove(handler);

        public void Invoke<T>(in T args) => Event<T>.Invoke(args);

        public void AddAsync<T>(IAsyncEvent<T> instance) => AsyncEvent<T>.Add(instance);
        public void RemoveAsync<T>(IAsyncEvent<T> instance) => AsyncEvent<T>.Remove(instance);

        public void AddAsync<T>(Func<T, ETask> handler) => AsyncEvent<T>.Add(handler);
        public void RemoveAsync<T>(Func<T, ETask> handler) => AsyncEvent<T>.Remove(handler);

        public ETask InvokeAsync<T>(T args) => AsyncEvent<T>.InvokeAsync(args);
    }
}
