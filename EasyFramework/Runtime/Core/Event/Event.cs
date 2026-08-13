/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/6
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyFramework
{
    // [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    // public sealed class EventDispatcherAttribute : Attribute
    // {
    //     public readonly Type DispatcherType;
    //     public EventDispatcherAttribute(Type type)
    //     {
    //         DispatcherType = type;
    //     }
    // }
    // public interface IEventDispatcher
    // {
    // }
    // public static class EventDispatcherExtensions
    // {
    //     public static void Invoke<T>(this IEventDispatcher dispatcher, in T t) where T : IEvent
    //     {
    //         Event<T>.Invoke(t, dispatcher.GetType());
    //     }
    // }

    public interface IEvent
    {
        
    }
    public interface IEvent<T> where T : IEvent
    {
        void Execute(in T args);
    }

    internal static class Event<T> where T : IEvent
    {
        private static readonly List<IEvent<T>> EventList = new();
        // private static readonly Type DispatcherType;
        
        // static Event()
        // {
        //     var attribute = EasyFrameworkReflection.GetCustomAttribute<EventDispatcherAttribute>(typeof(T));
        //     DispatcherType = attribute?.DispatcherType;
        // }

        public static void Add(IEvent<T> handler)
        {
            if (!EventList.Contains(handler)) EventList.Add(handler);
        }
        public static void Remove(IEvent<T> handler)
        {
            if (EventList.Contains(handler)) EventList.Remove(handler);
        }

        public static void Invoke(in T args, Type dispatcher = null)
        {
            // if (DispatcherType != null)
            // {
            //     if (dispatcher == null || !DispatcherType.IsAssignableFrom(dispatcher))
            //     {
            //         FDebug.LogError($"Invoke event error. dispatcher type not match: {DispatcherType.Name}");
            //         return;
            //     }
            // }

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
        }
    }
    
    public class Event : Singleton<Event>
    {
        public Event()
        {
            var eventObjects = EasyFrameworkReflection.CreateObjectsByAttribute(typeof(EventObjectAttribute));
        }
        
        public void Invoke<T>(in T args) where T : IEvent
        {
            Event<T>.Invoke(args);
        }
        
        public void Add<T>(IEvent<T> instance) where T : IEvent => Event<T>.Add(instance);
        public void Add(object instance)
        {
            var type = instance.GetType();
            var interfaces = type.GetInterfaces();

            foreach (var i in interfaces)
            {
                if (!i.IsGenericType)
                    continue;

                if (i.GetGenericTypeDefinition() != typeof(IEvent<>))
                    continue;

                var argType = i.GetGenericArguments()[0];

                var method = typeof(Event)
                    .GetMethod(nameof(AddGeneric), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(argType);

                method.Invoke(null, new[] { instance });
            }
        }
        
        public void Remove<T>(IEvent<T> instance) where T : IEvent => Event<T>.Remove(instance);
        public void Remove(object instance)
        {
            var type = instance.GetType();
            var interfaces = type.GetInterfaces();

            foreach (var i in interfaces)
            {
                if (!i.IsGenericType)
                    continue;

                if (i.GetGenericTypeDefinition() != typeof(IEvent<>))
                    continue;

                var argType = i.GetGenericArguments()[0];

                var method = typeof(Event)
                    .GetMethod(nameof(RemoveGeneric), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(argType);

                method.Invoke(null, new[] { instance });
            }
        }

        private static void AddGeneric<T>(object instance) where T : IEvent
        {
            Event<T>.Add((IEvent<T>)instance);
        }
        private static void RemoveGeneric<T>(object instance) where T : IEvent
        {
            Event<T>.Remove((IEvent<T>)instance);
        }
    }
}