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

        public static void Add(IEvent<T> handler)
        {
            if (handler == null) return;
            if (!EventList.Contains(handler)) EventList.Add(handler);
        }

        public static void Remove(IEvent<T> handler)
        {
            if (handler == null) return;
            if (EventList.Contains(handler)) EventList.Remove(handler);
        }

        public static void Add(Action<T> handler)
        {
            if (handler == null) return;
            if (!ActionList.Contains(handler)) ActionList.Add(handler);
        }

        public static void Remove(Action<T> handler)
        {
            if (handler == null) return;
            if (ActionList.Contains(handler)) ActionList.Remove(handler);
        }

        public static void Invoke(in T args)
        {
            // Invoke IEvent handlers
            var handlers = EventList;
            int count = handlers.Count;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    handlers[i].Execute(args);
                }
                catch (Exception e)
                {
                    FDebug.LogError(e);
                }
            }

            // Invoke Action handlers
            var actions = ActionList;
            int actionCount = actions.Count;

            for (int i = 0; i < actionCount; i++)
            {
                try
                {
                    actions[i]?.Invoke(args);
                }
                catch (Exception e)
                {
                    FDebug.LogError(e);
                }
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
    }
}