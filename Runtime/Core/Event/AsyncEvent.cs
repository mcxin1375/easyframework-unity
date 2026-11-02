/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/11/2
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public interface IAsyncEvent<T>
    {
        ETask ExecuteAsync(T args);
    }

    internal static class AsyncEvent<T>
    {
        private static readonly List<IAsyncEvent<T>> EventList = new();
        private static readonly List<Func<T, ETask>> ActionList = new();

        public static void Add(IAsyncEvent<T> handler)
        {
            if (handler == null) return;
            if (!EventList.Contains(handler)) EventList.Add(handler);
        }

        public static void Remove(IAsyncEvent<T> handler)
        {
            if (handler == null) return;
            EventList.Remove(handler);
        }

        public static void Add(Func<T, ETask> handler)
        {
            if (handler == null) return;
            if (!ActionList.Contains(handler)) ActionList.Add(handler);
        }

        public static void Remove(Func<T, ETask> handler)
        {
            if (handler == null) return;
            ActionList.Remove(handler);
        }

        public static async ETask InvokeAsync(T args)
        {
            // Awaiting dispatches can finish in any order, so each owns its snapshot.
            var snapshot = ObjectPool<Snapshot>.Shared.Rent();
            try
            {
                snapshot.Handlers.AddRange(EventList);
                snapshot.Actions.AddRange(ActionList);

                for (int i = 0; i < snapshot.Handlers.Count; i++)
                {
                    try
                    {
                        await snapshot.Handlers[i].ExecuteAsync(args);
                    }
                    catch (Exception e)
                    {
                        FDebug.LogError(e);
                    }
                }

                for (int i = 0; i < snapshot.Actions.Count; i++)
                {
                    try
                    {
                        await snapshot.Actions[i].Invoke(args);
                    }
                    catch (Exception e)
                    {
                        FDebug.LogError(e);
                    }
                }
            }
            finally
            {
                ObjectPool<Snapshot>.Shared.Return(snapshot);
            }
        }

        private sealed class Snapshot : IObjectPoolEvent
        {
            public readonly List<IAsyncEvent<T>> Handlers = new();
            public readonly List<Func<T, ETask>> Actions = new();

            public void OnReturn()
            {
                Handlers.Clear();
                Actions.Clear();
            }
        }
    }
}
