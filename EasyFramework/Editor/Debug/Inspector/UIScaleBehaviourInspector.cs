/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/8/15
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(UIScaleBehaviour))]
    public class UIScaleBehaviourInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var behaviour = target as UIScaleBehaviour;
            if (behaviour.editorRefresh) behaviour.Refresh();
        }
    }
}