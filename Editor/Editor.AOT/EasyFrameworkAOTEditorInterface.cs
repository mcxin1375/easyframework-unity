/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/7/30
// describe:
//----------------------------------------------------------------*/


namespace EasyFramework.Editor
{
    public class EasyFrameworkAOTEditor : IEasyFrameworkAOTEditor
    {
        
#if EF_HYBRIDCLR
        public HybridCLRBuilderVersion HybridCLRVersionInfo { get; } = HybridCLRBuilder.Instance.CreateHybridCLRVersionInfo();
#endif
    }
}
