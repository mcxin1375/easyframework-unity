using UnityEditor;
using UnityEngine;


namespace EasyFramework.Editor
{
    public static class EditorGUILayoutHelper
    {
        public static void DrawExtensions<T>(T[] extensions) where T : IEditorToolExtension
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