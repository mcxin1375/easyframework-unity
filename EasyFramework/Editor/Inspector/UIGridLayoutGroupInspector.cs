/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/8/15
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(UIGridBehaviour))]
    public class UIGridLayoutGroupInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var grid = target as UIGridBehaviour;
            grid.RefreshOnEditorMode();
        }
    }
}