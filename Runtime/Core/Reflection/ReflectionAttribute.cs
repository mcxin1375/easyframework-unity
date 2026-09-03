/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public enum ReflectionMode
    {
        Attribute = 1,
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ReflectionAttribute : Attribute
    {
        public ReflectionMode Mode { get; }

        public ReflectionAttribute(ReflectionMode mode)
        {
            Mode = mode;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ReflectionOrderAttribute : Attribute
    {
        public readonly int Order;
        public ReflectionOrderAttribute(int order)
        {
            Order = order;
        }
    }
    
    [AttributeUsage(AttributeTargets.Assembly)]
    public class EasyFrameworkReflectionAttribute : Attribute { }
}
