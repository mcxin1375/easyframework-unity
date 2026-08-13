using System;

namespace EasyFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ObjectEventSystemAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class ObjectEventSystemInterfaceAttribute : Attribute
    {
        public readonly Type ObjectType;
        public ObjectEventSystemInterfaceAttribute(Type objectType)
        {
            ObjectType = objectType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ObjectEventSystemOrderAttribute : Attribute
    {
        public readonly Type InterfaceType;
        public readonly int Order;
        public ObjectEventSystemOrderAttribute(Type interfaceType, int order)
        {
            InterfaceType = interfaceType;
            Order = order;
        }
    }
}
