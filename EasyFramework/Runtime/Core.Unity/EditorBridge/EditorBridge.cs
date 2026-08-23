/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/

#if UNITY_EDITOR

using UnityEngine;

namespace EasyFramework
{
    public interface IEditorBridgeObject
    {
        IResLoader ResLoader { get; }
        T LoadProjectSetting<T>() where T : ProjectSettings<T>;
    }

    internal static class EditorBridge
    {
        public static IResLoader ResLoader => Instance.ResLoader;
        
        public static T LoadProjectSetting<T>() where T : ProjectSettings<T> => Instance.LoadProjectSetting<T>();
        
        private static readonly IEditorBridgeObject Instance = EasyFrameworkReflection.CreateInstance<IEditorBridgeObject>();
    }
}

#endif