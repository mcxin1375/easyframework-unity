using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class ToolDrawHelper
    {
        public static void DrawTools<T>(T[] tools) where T : ITool
        {
            if (tools == null) return;
            
            foreach (var tool in tools)
            {
                // EditorGUILayout.HelpBox($"Tool: {tool.GetType().Name}  Order: {tool.Order}", MessageType.Info);
                // EditorGUILayout.LabelField($"[{tool.Order}]  {tool.GetType().Name}", GUIStyles.MainStyle);
                EditorGUILayout.LabelField($"[{tool.GetType().Name}]", GUIStyles.MainStyle);
                DrawToolExtensions(tool.Extension);
                
                EditorGUILayout.Space(5);
            }
        }
        
        public static void DrawToolExtensions<T>(T[] extensions) where T : IToolExtension
        {
            if (extensions == null) return;
            
            foreach (var ex in extensions)
            {
                if (ex is ScriptableObject o)
                {
                    EditorGUILayout.ObjectField($"{ex.Order}", o, o.GetType(), false);
                }
                else
                {
                    EditorGUILayout.LabelField($"{ex.Order}", ex.GetType().Name);
                }
            }
        }
        
        public static void DrawExtensions<T>(T[] extensions) where T : IToolExtension
        {
            EditorGUILayout.HelpBox($"{typeof(T).Name}", MessageType.Info);
            
            if (extensions == null) return;
            
            foreach (var ex in extensions)
            {
                if (ex is ScriptableObject o)
                {
                    EditorGUILayout.ObjectField($"{ex.Order}", o, o.GetType(), false);
                }
                else
                {
                    EditorGUILayout.LabelField($"{ex.Order}", ex.GetType().Name);
                }
            }
        }
    }
}