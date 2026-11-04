/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/8/15
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(UIRootBehaviour))]
    public class UIRootBehaviourInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var behaviour = target as UIRootBehaviour;
            behaviour.UpdateRootSettings();
            
            if (!Application.isPlaying || !WindowManager.HasInstance()) return;
            EditorGUILayout.HelpBox("UILayerData", MessageType.Info);
            foreach (var kv in WindowManager.Instance.UILayerWindowDict)
            {
                EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}UILayer.{kv.Key}", GUIStyles.MainStyle);
                foreach (var window in kv.Value.WindowList)
                {
                    EditorGUILayout.LabelField($"{GUIStyles.DependencyPrefix}{window.GetType().Name}", GUIStyles.DependencyStyle);
                }
                EditorGUILayout.Space(5);
            }
        }
    }
}
