
namespace EasyFramework.Editor
{
    public class EditorBridgeObject : IEditorBridgeObject
    {
        public IResLoader ResLoader => AssetBundleLoaderEditor.Instance;
        
#if EF_HYBRIDCLR
        // public HybridCLRBuilderVersion HybridCLRBuilderVersion { get; }
        public HybridCLRBuilderVersion HybridCLRBuilderVersion => HybridCLRBuilder.Instance.CreateHybridCLRVersionInfo();
#endif
        
        public T LoadProjectSetting<T>() where T : ProjectSettings<T> => UnityEditorHelper.LoadProjectSettings<T>();
    }
}