/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/8/15
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(UIWindowBehaviour))]
    public class UIWindowBehaviourInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var behaviour = target as UIWindowBehaviour;
            
            EditorGUILayout.HelpBox("UILayerData", MessageType.Info);
            foreach (var kv in behaviour.UILayerWindowDict)
            {
                EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}UILayer.{kv.Key}", GUIStyles.MainStyle);
                foreach (var window in kv.Value)
                {
                    EditorGUILayout.LabelField($"{GUIStyles.DependencyPrefix}{window.GetType().Name} - IsOpen: {window.IsOpen}", GUIStyles.DependencyStyle);
                }
                EditorGUILayout.Space(5);
            }
            
            // if (GUILayout.Button("UpdateRootSettings", GUILayout.Height(50)))
            // {
            //     behaviour.UpdateRootSettings();
            // }
        }
    }
}