/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class UISystemProvider : ProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<UISystemProvider>.Instance;
        
        public UISystemProvider() : base(EasyFrameworkProvider.ToChildProvider<WindowManager>()) { }


        protected override void OnDrawSettings(string searchContext)
        {
            if (!WindowManager.HasInstance()) return;
            
            EditorGUILayout.HelpBox("UILayerData", MessageType.Info);
            foreach (var kv in WindowManager.Instance.UIWindowBehaviour.UILayerWindowDict)
            {
                EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}UILayer.{kv.Key}", GUIStyles.MainStyle);
                foreach (var window in kv.Value)
                {
                    EditorGUILayout.LabelField($"{GUIStyles.DependencyPrefix}{window.GetType().Name} - IsOpen: {window.IsOpen}", GUIStyles.DependencyStyle);
                }
                EditorGUILayout.Space(5);
            }
        }
    }
}