/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/7/30
// describe:
//----------------------------------------------------------------*/


namespace EasyFramework.Editor
{
    public class EasyFrameworkAOTEditor : IEasyFrameworkAOTEditor
    {
        public HybridCLRBuilderVersion HybridCLRVersionInfo { get; } = HybridCLRBuilder.Instance.CreateHybridCLRVersionInfo();
    }
}
