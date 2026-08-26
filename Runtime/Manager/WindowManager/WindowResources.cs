/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;
using System.Reflection;
using UnityEngine;

namespace EasyFramework
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
    
    public abstract class WindowResources : Window
    {
        protected sealed override GameObject CreateWindowObject(Transform parent)
        {
            var attribute = Type.GetCustomAttribute<WindowResourcesPathAttribute>();
            var resourcePath = attribute?.Path ?? Type.Name;
            return F.ResLoader.CreateObjResources(resourcePath, parent);
        }

        protected sealed override ETask<GameObject> CreateWindowObjectAsync(Transform parent)
        {
            var attribute = Type.GetCustomAttribute<WindowResourcesPathAttribute>();
            var resourcePath = attribute?.Path ?? Type.Name;
            return ETask.FromResult(F.ResLoader.CreateObjResources(resourcePath, parent));
        }
    }
}