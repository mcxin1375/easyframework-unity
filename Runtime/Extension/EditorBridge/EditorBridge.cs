/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/

#if UNITY_EDITOR

namespace EasyFramework
{
    public interface IEditorBridgeObject
    {
        IResLoader ResLoader { get; }
        
#if EF_HYBRIDCLR
        HybridCLRBuilderVersion HybridCLRBuilderVersion { get; }
#endif
        
        T LoadProjectSetting<T>() where T : ProjectSettings<T>;
    }

    internal static class EditorBridge
    {
        public static IResLoader ResLoader => Instance.ResLoader;
        
#if EF_HYBRIDCLR
        public static HybridCLRBuilderVersion HybridCLRBuilderVersion => Instance.HybridCLRBuilderVersion;
#endif
        
        public static T LoadProjectSetting<T>() where T : ProjectSettings<T> => Instance.LoadProjectSetting<T>();
        
        private static readonly IEditorBridgeObject Instance = EasyFrameworkReflection.CreateInstance<IEditorBridgeObject>();
    }
}

#endif