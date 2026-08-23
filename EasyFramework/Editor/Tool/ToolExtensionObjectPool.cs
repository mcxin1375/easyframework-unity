/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class ToolExtensionObjectPool
    {
        public static readonly Type[] AllTypes = EasyFrameworkReflection.FindInstanceTypes<IToolExtension>();

        private static readonly Dictionary<Type, object[]> InstanceDict = new();
        private static readonly Dictionary<Type, object[]> ScriptableDict = new();
        private static readonly List<object> TMPList = new();

        internal static object[] GetInstanceObjects<T>() where T : IToolExtension
        {
            var type = typeof(T);
            if (!InstanceDict.TryGetValue(type, out var objects))
            {
                TMPList.Clear();

                foreach (var targetType in AllTypes)
                {
                    if (!type.IsAssignableFrom(targetType)) continue;

                    if (typeof(ITool).IsAssignableFrom(targetType))
                    {
                        var instance = EasyFrameworkReflection.FindFieldOrProperty<ITool>(targetType);
                        if (instance != null)
                        {
                            TMPList.Add(instance);
                            continue;
                        }
                    }

                    if (!typeof(ScriptableObject).IsAssignableFrom(targetType))
                    {
                        TMPList.Add(EasyFrameworkReflection.CreateObject(targetType));
                    }
                }

                objects = TMPList.ToArray();
                InstanceDict[type] = objects;
            }

            return objects;
        }

        internal static object[] GetScriptableObjects<T>(bool forceRefresh = false) where T : IToolExtension
        {
            var type = typeof(T);
            if (forceRefresh || !ScriptableDict.TryGetValue(type, out var objects))
            {
                TMPList.Clear();

                foreach (var targetType in AllTypes)
                {
                    if (typeof(ScriptableObject).IsAssignableFrom(targetType) && type.IsAssignableFrom(targetType))
                    {
                        var scriptableObjects = UnityEditorHelper.FindAssetsByType(targetType);
                        TMPList.AddRange(scriptableObjects);
                    }
                }

                objects = TMPList.ToArray();
                ScriptableDict[type] = objects;
            }

            return objects;
        }
    }
}
