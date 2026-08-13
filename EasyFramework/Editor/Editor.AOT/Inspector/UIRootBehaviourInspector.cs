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
            // if (GUILayout.Button("UpdateRootSettings", GUILayout.Height(50)))
            // {
            //     behaviour.UpdateRootSettings();
            // }
        }
    }
}