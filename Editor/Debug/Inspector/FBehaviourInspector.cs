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

            DrawInfo();
        }

        public static void DrawInfo()
        {
            // EditorGUILayout.HelpBox("World", MessageType.Info);
            // if (FWorld.HasMainWorld())
            // {
            //     foreach (var system in FWorld.Main.SystemList)
            //     {
            //         EditorGUILayout.LabelField($"{system.Order}", system.GetType().Name);
            //     }
            // }

            EditorGUILayout.HelpBox("ControllerManager", MessageType.Info);
            foreach (var controller in ControllerManager.Instance.EnterList)
            {
                EditorGUILayout.LabelField($"{controller.GetType().Name}", $"IsActive: {controller.IsActive}");
            }

            EditorGUILayout.HelpBox("SceneLoader", MessageType.Info);
            foreach (var value in SceneLoader.Instance.SceneDict.Values)
            {
                EditorGUILayout.LabelField($"{value.SceneName}",
                    $"State: {value.State}, IsActive: {value.IsActive}, Alive: {value.Alive}");
            }

            EditorGUILayout.HelpBox("SpriteLoader", MessageType.Info);
            foreach (var value in SpriteLoader.Instance.AtlasDict.Values)
            {
                EditorGUILayout.LabelField($"{value.AtlasName}", $"Alive: {value.Alive}");
            }
        }
    }
}