/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public static class WindowContainer
    {
        private static readonly Dictionary<Type, IWindow> WindowDict = new();
        
        public static T GetWindow<T>() where T : class, IWindow => GetWindow(typeof(T)) as T;
        public static IWindow GetWindow(Type type) => WindowDict.GetValueOrDefault(type);
        
        public static T GetOrCreateWindow<T>() where T : class, IWindow, new()
        {
            var type = typeof(T);
            if (!WindowDict.TryGetValue(type, out var window))
            {
                window = new T();
                WindowDict[type] = window;
            }
            return window as T;
        }
        public static IWindow GetOrCreateWindow(Type type)
        {
            if (!WindowDict.TryGetValue(type, out var window))
            {
                window = Activator.CreateInstance(type) as IWindow;
                WindowDict[type] = window;
            }
            return window;
        }
    }
}