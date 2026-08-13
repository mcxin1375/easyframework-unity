using System;

namespace EasyFramework.AOT
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowResourcesPathAttribute : Attribute
    {
        public readonly string Path;
        public WindowResourcesPathAttribute(string path)
        {
            Path = path;
        }
    }
}