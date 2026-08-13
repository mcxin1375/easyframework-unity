/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

#if UNITY_EDITOR

namespace EasyFramework
{
    public interface IEditorBridgeObject
    {
        IResLoader ResLoader { get; }
        
        void Initialize();
        T LoadProjectSetting<T>() where T : ScriptableObject;
    }

    internal static class EditorBridge
    {
        public static readonly IEditorBridgeObject Instance = EasyFrameworkReflection.CreateInstance<IEditorBridgeObject>();

        public static void Initialize()
        {
            Instance.Initialize();
        }
    }
}

#endif