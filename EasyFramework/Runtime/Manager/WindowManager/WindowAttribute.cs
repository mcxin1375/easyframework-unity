using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyFramework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class WindowConfigAttribute : Attribute
    {
        private static readonly Dictionary<Type, WindowConfigAttribute> _cache = new();

        public UILayer Layer = UILayer.HUD;
        public Type OpenBeforeType;
        public float DestroyTime = 60;

        public WindowConfigAttribute() { }

        internal static WindowConfigAttribute GetConfig(Type windowType)
        {
            if (!_cache.TryGetValue(windowType, out var config))
            {
                config = windowType.GetCustomAttribute<WindowConfigAttribute>() ?? new WindowConfigAttribute();
                _cache.Add(windowType, config);
            }
            return config;
        }
    }
}
