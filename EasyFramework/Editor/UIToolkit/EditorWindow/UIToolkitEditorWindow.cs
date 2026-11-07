/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/28
// describe: 
//----------------------------------------------------------------*/

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyFramework.Editor
{
    public abstract class UIToolkitEditorWindow<T> : UIToolkitEditorWindow where T : EditorWindow
    {
        protected override void OnCreateGUI()
        {
            string fileName = typeof(T).Name;
            string[] arr = AssetDatabase.FindAssets(fileName);
            if (arr?.Length > 0)
            {
                foreach (string s in arr)
                {
                    string path = AssetDatabase.GUIDToAssetPath(s);
                    Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                    if (type == typeof(VisualTreeAsset))
                    {
                        VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
                        if (asset != null) rootVisualElement.Add(asset.Instantiate());
                        break;
                    }
                }
            }
        }

        public static void Open()
        {
            T window = GetWindow<T>();
            window.titleContent = new GUIContent(typeof(T).Name);
            window.Show();
        }
        public static void Open(Vector2 minSize)
        {
            T window = GetWindow<T>();
            window.titleContent = new GUIContent(typeof(T).Name);
            window.minSize = minSize;
            window.Show();
        }
        public static void Open(string titleContent, Vector2 minSize)
        {
            T window = GetWindow<T>();
            window.titleContent = new GUIContent(titleContent);
            window.minSize = minSize;
            window.Show();
        }
        public static void Close()
        {
            T window = GetWindow<T>();
            window.Close();
        }
    }

    public abstract class UIToolkitEditorWindow : EditorWindow
    {
        protected virtual string UxmlPath => string.Empty;
        
        private UIToolkitEditorWindowEx[] _windowExes;
        
        private void CreateGUI()
        {
            // Log.Info("CreateGUI", IsOpen);

            OnCreateGUI();
            
            _windowExes = ReflectionUtility.FindFieldsAndProperties<UIToolkitEditorWindowEx>(this);
            UnityHelper.AutoSetUIToolkitElement(this, rootVisualElement);
            if (_windowExes?.Length > 0)
            {
                foreach (UIToolkitEditorWindowEx baseWindowEx in _windowExes)
                {
                    UnityHelper.AutoSetUIToolkitElement(baseWindowEx, rootVisualElement);
                    baseWindowEx.Create(this);
                }
            }
            rootVisualElement.Query<Button>().ForEach(btn => btn.clicked += () =>
            {
                OnButtonClick(btn);
                if (_windowExes?.Length > 0) foreach (UIToolkitEditorWindowEx baseWindowEx in _windowExes) baseWindowEx?.ButtonClick(btn);
            });
            
            OnAddListeners();
            if (_windowExes?.Length > 0) foreach (UIToolkitEditorWindowEx baseWindowEx in _windowExes) baseWindowEx?.AddListeners();
            
            OnOpen();
            if (_windowExes?.Length > 0) foreach (UIToolkitEditorWindowEx baseWindowEx in _windowExes) baseWindowEx?.Open();
        }

        private void OnDestroy()
        {
            // Log.Info("OnDestroy");
            if (_windowExes?.Length > 0) foreach (UIToolkitEditorWindowEx baseWindowEx in _windowExes) baseWindowEx?.Dispose();
            
            OnRemoveListeners();
            if (_windowExes?.Length > 0) foreach (UIToolkitEditorWindowEx baseWindowEx in _windowExes) baseWindowEx?.RemoveListeners();
            
            OnClose();
            if (_windowExes?.Length > 0) foreach (UIToolkitEditorWindowEx baseWindowEx in _windowExes) baseWindowEx?.Close();
        }

        protected virtual void OnCreateGUI()
        {
            if (!string.IsNullOrWhiteSpace(UxmlPath))
            {
                VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
                if (asset != null) rootVisualElement.Add(asset.Instantiate());
            }
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnButtonClick(Button btn) { }

        public static void Open<T>() where T : EditorWindow
        {
            T window = CreateWindow<T>();
            window.titleContent = new GUIContent(typeof(T).Name);
            window.minSize = new Vector2(800, 600);
        }
        public static void Open<T>(string titleContent, Vector2 minSize) where T : EditorWindow
        {
            T window = CreateWindow<T>();
            window.titleContent = new GUIContent(titleContent);
            window.minSize = minSize;
        }
    }
}