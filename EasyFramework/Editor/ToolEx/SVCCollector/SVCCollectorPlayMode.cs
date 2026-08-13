/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public static class SVCCollectorPlayMode
    {
        private static bool _initialized = false;
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            
            // Debug.Log("ShaderVariantExtension Initializing");
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            var settings = SVCCollectorSettings.Instance;
            if (!settings.svcEnabled) return;
            
            if (settings.svcSavePlayState == obj)
            {
                SVCCollector.Instance.Execute();
            }
        }
    }
}