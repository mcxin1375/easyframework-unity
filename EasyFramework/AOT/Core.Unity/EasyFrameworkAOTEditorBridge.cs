/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/7/30
// describe:
//----------------------------------------------------------------*/

#if UNITY_EDITOR

namespace EasyFramework
{
    public interface IEasyFrameworkAOTEditor
    {
        HybridCLRBuilderVersion HybridCLRVersionInfo { get; }
    }
    
    internal static class EasyFrameworkAOTEditorBridge
    {
        public static HybridCLRBuilderVersion DllVersion => Instance.HybridCLRVersionInfo;
        
        private static IEasyFrameworkAOTEditor _instance;
        private static IEasyFrameworkAOTEditor Instance => _instance ??= EasyFrameworkReflection.CreateInstance<IEasyFrameworkAOTEditor>();
    }
}

#endif