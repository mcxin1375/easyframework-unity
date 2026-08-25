
namespace EasyFramework.Editor
{
    public class EditorBridgeObject : IEditorBridgeObject
    {
        public IResLoader ResLoader => AssetBundleLoaderEditor.Instance;
        public T LoadProjectSetting<T>() where T : ProjectSettings<T> => UnityEditorHelper.LoadProjectSettings<T>();
    }
}