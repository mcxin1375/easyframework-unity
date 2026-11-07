/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyFramework.Editor
{
    public abstract class ProjectSettingsProvider<T> : ProjectSettingsProvider where T : ProjectSettings<T>
    {
        protected T Settings { get; private set; }
        private readonly FieldInfo[] _fieldInfos = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);
        private SerializedObject _serializedObject;
        
        protected ProjectSettingsProvider(string path) : base(path) { }
        
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            SaveData();
        }
        protected override void OnFocusChanged(bool value)
        {
            if (value) OnRefresh();
            else SaveData();
        }
        protected override void OnRefresh()
        {
            Settings = ProjectSettings<T>.ReloadEditorOnly();
            _serializedObject = Settings != null ? new SerializedObject(Settings) : null;
        }

        private void SaveData()
        {
            Settings?.SaveEx();
        }

        protected override void OnDrawSettings(string searchContext)
        {
            if (_serializedObject != null && _serializedObject.targetObject != null)
            {
                _serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                    
                // EditorGUILayout.HelpBox($"{objInfo.ScriptableObj.GetType().Name}", MessageType.Info);
                foreach (var fieldInfo in _fieldInfos)
                {
                    var p = _serializedObject.FindProperty(fieldInfo.Name);
                    if (p == null) continue;
                    DrawSettingsProperty(p);
                }
                    
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                    SaveData();
                    OnSettingsChanged();
                }
            }
        }

        protected virtual void DrawSettingsProperty(SerializedProperty property) => EditorGUILayout.PropertyField(property);
        protected virtual void OnSettingsChanged() { }
    }
    
    public abstract class ProjectSettingsProvider : SettingsProvider
    {
        protected ProjectSettingsProvider(string path) : base(path, SettingsScope.Project) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            EditorApplication.focusChanged += EditorApplicationOnfocusChanged;
            OnRefresh();
        }
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EditorApplication.focusChanged -= EditorApplicationOnfocusChanged;
        }
        private void EditorApplicationOnfocusChanged(bool obj)
        {
            OnFocusChanged(obj);
            Repaint();
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            // using (CreateSettingsWindowGUIScope())
            // {
            DrawSettings(searchContext);
            OnDrawSettingsBefore(searchContext);
            
            // Rect r = EditorGUILayout.BeginVertical(SettingsFrameBox);
            // EditorGUILayout.Space(5);
            OnDrawSettings(searchContext);
            // EditorGUILayout.EndVertical();
                
            OnDrawSettingsAfter(searchContext);
            // }
        }
        
        protected virtual void OnRefresh() { }
        protected virtual void OnFocusChanged(bool value) { }
        
        
        protected virtual void DrawSettings(string searchContext) { }
        
        protected virtual void OnDrawSettingsBefore(string searchContext) { }
        protected virtual void OnDrawSettings(string searchContext) { }
        protected virtual void OnDrawSettingsAfter(string searchContext) { }

        protected IDisposable CreateSettingsWindowGUIScope()
        {
            var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
            return Activator.CreateInstance(type) as IDisposable;
        }
        
        protected readonly GUIStyle SettingsFrameBox = new GUIStyle(GUIStyles.FrameBox) { padding = new RectOffset(1, 1, 1, 0) };
        
        protected int DrawTabGUI(Rect rect, string[] contents, int tabIndex)
        {
            GUIStyle buttonStyle = null;
            for (int i = 0; i < contents.Length; i++)
            {
                Rect buttonRect = GUIStyles.GetTabRect(rect, i, contents.Length, out buttonStyle);
                if (GUI.Toggle(buttonRect, i == tabIndex, contents[i], buttonStyle)) tabIndex = i;
            }
            return tabIndex;
        }
        
        protected int DrawTabGUI(Rect rect, GUIContent[] contents, int tabIndex, System.Action<int> action = null)
        {
            GUIStyle buttonStyle = null;
            for (int i = 0; i < contents.Length; i++)
            {
                Rect buttonRect = GUIStyles.GetTabRect(rect, i, contents.Length, out buttonStyle);
                if (GUI.Toggle(buttonRect, i == tabIndex, contents[i], buttonStyle)) tabIndex = i;
            }
            return tabIndex;
        }
        
        private const float LineThickness = 1f;
        private static Color LineColor = Color.black;
        protected void DrawLine()
        {
            Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth, LineThickness);
            rect.x = 0; // 从左侧边缘开始
            EditorGUI.DrawRect(rect, LineColor);
        }

    }
}