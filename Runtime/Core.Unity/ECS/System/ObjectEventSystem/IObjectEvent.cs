// using System;
//
// namespace EasyFramework
// {
//     public interface IObjectEvent
//     {
//         
//     }
//
//     public static class ObjectEventSystemEx
//     {
//         public static void Invoke<TEvent>(this IObjectEvent t, Action<TEvent> action) where TEvent : class
//         {
//             F.ObjectEventSystem.Invoke(t.GetType(), action);
//         }
//
//         public static TEvent[] GetEvents<TEvent>(this IObjectEvent t) where TEvent : class
//         {
//             return F.ObjectEventSystem.GetEvents<TEvent>(t.GetType());
//         }
//     }
// }
