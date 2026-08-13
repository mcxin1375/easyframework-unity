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
        
#if EF_HYBRIDCLR
        HybridCLRBuilderVersion HybridCLRVersionInfo { get; }
#endif
        
    }
    
    internal static class EasyFrameworkAOTEditorBridge
    {
        
#if EF_HYBRIDCLR
        public static HybridCLRBuilderVersion DllVersion => Instance.HybridCLRVersionInfo;
#endif
        
        private static IEasyFrameworkAOTEditor _instance;
        private static IEasyFrameworkAOTEditor Instance => _instance ??= EasyFrameworkReflection.CreateInstance<IEasyFrameworkAOTEditor>();
    }
}

#endif