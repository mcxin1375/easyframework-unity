using System;

namespace EasyFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WindowOpenBeforeAttribute : Attribute
    {
        public readonly Type WindowType;
        public WindowOpenBeforeAttribute(Type windowType)
        {
            WindowType = windowType;
        }
    }
}
