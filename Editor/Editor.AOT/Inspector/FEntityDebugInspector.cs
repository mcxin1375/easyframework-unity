using System.Reflection;
using EasyFramework;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(FEntityDebug))]
    public class FEntityDebugInspector : UnityEditor.Editor
    {
        // 折叠状态缓存（每种组件类型一个）
        private readonly System.Collections.Generic.Dictionary<string, bool> _foldoutStates = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var behaviour = target as FEntityDebug;
            if (behaviour == null || behaviour.Entity == null) return;

            var components = behaviour.Entity.GetComponents();
            foreach (var component in components)
            {
                string typeName = component.GetType().Name;
                if (!_foldoutStates.ContainsKey(typeName))
                    _foldoutStates[typeName] = true;

                _foldoutStates[typeName] = EditorGUILayout.Foldout(_foldoutStates[typeName], typeName, true, EditorStyles.foldoutHeader);

                if (!_foldoutStates[typeName]) continue;

                EditorGUILayout.BeginVertical("box");
                DrawComponentFields(component);
                DrawComponentProperties(component);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawComponentFields(object component)
        {
            var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var value = field.GetValue(component);
                DrawValue(field.Name, value, field.FieldType);
            }
        }

        private void DrawComponentProperties(object component)
        {
            var properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0) continue;

                var value = property.GetValue(component, null);
                DrawValue(property.Name, value, property.PropertyType);
            }
        }

        private void DrawValue(string name, object value, System.Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                EditorGUILayout.ObjectField(name, value as UnityEngine.Object, type, true);
            }
            else if (type == typeof(int))
            {
                EditorGUILayout.IntField(name, value != null ? (int)value : 0);
            }
            else if (type == typeof(float))
            {
                EditorGUILayout.FloatField(name, value != null ? (float)value : 0f);
            }
            else if (type == typeof(bool))
            {
                EditorGUILayout.Toggle(name, value != null && (bool)value);
            }
            else if (type == typeof(string))
            {
                EditorGUILayout.TextField(name, value as string ?? string.Empty);
            }
            else if (type.IsEnum)
            {
                EditorGUILayout.EnumPopup(name, (System.Enum)value);
            }
            else if (type == typeof(Vector2))
            {
                EditorGUILayout.Vector2Field(name, value != null ? (Vector2)value : Vector2.zero);
            }
            else if (type == typeof(Vector3))
            {
                EditorGUILayout.Vector3Field(name, value != null ? (Vector3)value : Vector3.zero);
            }
            else if (type == typeof(Vector4))
            {
                EditorGUILayout.Vector4Field(name, value != null ? (Vector4)value : Vector4.zero);
            }
            else if (type == typeof(Color))
            {
                EditorGUILayout.ColorField(name, value != null ? (Color)value : Color.white);
            }
            else
            {
                // fallback
                EditorGUILayout.LabelField(name, value?.ToString() ?? "null");
            }
        }
    }
}
