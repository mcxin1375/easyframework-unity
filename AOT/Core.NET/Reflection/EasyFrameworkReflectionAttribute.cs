using System;

namespace EasyFramework
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class EasyFrameworkReflectionAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class EasyFrameworkReflectionOrderAttribute : Attribute
    {
        public readonly int Order;
        public EasyFrameworkReflectionOrderAttribute(int order)
        {
            Order = order;
        }
    }
}