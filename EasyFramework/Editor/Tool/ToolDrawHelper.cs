using UnityEditor;
using UnityEngine;


namespace EasyFramework.Editor
{
    public static class ToolDrawHelper
    {
        public static void DrawExtensions<T>(IToolEvent<T>[] extensions) where T : SingletonTool<T>, new()
        {
            EditorGUILayout.HelpBox($"Type: {typeof(T).Name}", MessageType.Info);

            if (extensions == null) return;
            
            foreach (var ex in extensions)
            {
                if (ex is ScriptableObject o)
                {
                    EditorGUILayout.ObjectField($"Order: {ex.Order}", o, o.GetType(), false);
                }
                else
                {
                    EditorGUILayout.LabelField($"Order: {ex.Order}", ex.GetType().Name);
                }
            }
        }
    }
}