
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EditorBridgeObject : IEditorBridgeObject
    {
        public IResLoader ResLoader {
            get
            {
                switch (EasyFrameworkEditorSettings.Instance.resLoaderMode)
                {
                    case EResLoaderMode.Editor:
                        return AssetBundleLoaderEditor.Instance;
                }
                return AssetBundleLoader.Instance;
            }
        }

        public void Initialize()
        {
            
        }

        public T LoadProjectSetting<T>() where T : ProjectSettings<T> => UnityEditorHelper.LoadProjectSettings<T>();
    }
}