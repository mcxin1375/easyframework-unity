/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/8/15
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(UIAdapterBehaviour))]
    public class UIAdapterBehaviourInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var behaviour = target as UIAdapterBehaviour;
            if (behaviour.editorRefresh) behaviour.Refresh();
        }
    }
}