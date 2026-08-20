using UnityEditor;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(FBehaviour))]
    public class FBehaviourInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var behaviour = target as FBehaviour;
            if (behaviour == null) return;

            EditorGUILayout.LabelField($"World: {F.World.GetType().Name} Systems: {F.World.SystemList.Count}");
            foreach (var val in F.World.SystemList)
            {
                EditorGUILayout.LabelField($"{val.Order}", val.GetType().Name);
            }
        }
    }
}