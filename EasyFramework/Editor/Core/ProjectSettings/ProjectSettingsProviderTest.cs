// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace EasyFramework.Editor
// {
//     public class SettingsObjectInfo
//     {
//         public string Name => ScriptableObj.GetType().Name;
//         public string TabName { get; }
//         public string[] TabArr { get; }
//         public ScriptableObject ScriptableObj { get; }
//         public SerializedObject SerializedObject { get; private set; }
//         public FieldInfo[] FieldInfos { get; private set; }
//
//         public SettingsObjectInfo(ScriptableObject o)
//         {
//             ScriptableObj = o;
//             SerializedObject = new SerializedObject(o);
//             FieldInfos = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
//             
//             var attribute = EasyFrameworkReflection.GetCustomAttribute<ProjectSettingsTagAttribute>(o.GetType());
//             if (attribute != null)
//             {
//                 switch (attribute.SettingsTag)
//                 {
//                     case EProjectSettingsTag.Resources:
//                         TabName = $"{Name}(Res)";
//                         break;
//                     case EProjectSettingsTag.AssetBundle:
//                         TabName = $"{Name}(AB)";
//                         break;
//                     case EProjectSettingsTag.Editor:
//                         TabName = $"{Name}(Editor)";
//                         break;
//                 }
//             }
//             else
//             {
//                 TabName = Name;
//             }
//             TabArr = new string[] { TabName };
//         }
//     }
//
//     public abstract class ProjectSettingsProviderTest<T> : ProjectSettingsProvider where T : ProjectSettingsProviderTest<T>, new()
//     {
//         private static T _provider;
//         protected static T GetOrCreate()
//         {
//             // Debug.Log(typeof(T).Name);
//             
//             // var sw = new System.Diagnostics.Stopwatch();
//             // sw.Start();
//             if (_provider == null) _provider = new T();
//             // sw.Stop();
//             // string timeStr = sw.Elapsed.ToString(@"hh\:mm\:ss\.fff");
//             // Debug.Log($"{typeof(T).Name}: {timeStr}");
//             
//             return _provider;
//         }
//         protected ProjectSettingsProviderTest(string path) : base(path) { }
//     }
//     
//     public abstract class ProjectSettingsProvider : SettingsProvider
//     {
//         protected virtual bool DrawTab => true;
//         protected virtual int DefaultTabIndex => 0;
//         
//         private readonly List<SettingsObjectInfo> _objList = new List<SettingsObjectInfo>();
//         private string[] _tabArr;
//         private int _tabIndex;
//
//         protected string GUISearchContext { get; private set; }
//
//         protected ProjectSettingsProvider(string path) : base(path, SettingsScope.Project) { }
//
//         public override void OnActivate(string searchContext, VisualElement rootElement)
//         {
//             base.OnActivate(searchContext, rootElement);
//
//             _tabIndex = DefaultTabIndex;
//             
//             EditorApplication.focusChanged += EditorApplicationOnfocusChanged;
//             
//             LoadData();
//         }
//
//         private void EditorApplicationOnfocusChanged(bool obj)
//         {
//             OnfocusChanged(obj);
//             Repaint();
//         }
//
//         public override void OnDeactivate()
//         {
//             base.OnDeactivate();
//             
//             EditorApplication.focusChanged -= EditorApplicationOnfocusChanged;
//             
//             SaveData();
//             _objList.Clear();
//         }
//
//         public override void OnInspectorUpdate()
//         {
//             base.OnInspectorUpdate();
//             
//             if (_objList.Count == 0) return;
//             SettingsObjectInfo objInfo = _objList[_tabIndex];
//             if (objInfo.SerializedObject == null || objInfo.SerializedObject.targetObject == null)
//             {
//                 LoadData();
//             }
//         }
//
//         public override void OnGUI(string searchContext)
//         {
//             base.OnGUI(searchContext);
//
//             GUISearchContext = searchContext;
//
//             // using (CreateSettingsWindowGUIScope())
//             // {
//                 OnBeforeDraw();
//                 
//                 if (DrawTab)
//                 {
//                     if (_objList.Count > 0)
//                     {
//                         Rect r = EditorGUILayout.BeginVertical(SettingsFrameBox);
//                         _tabIndex = Mathf.Clamp(_tabIndex, 0, _objList.Count - 1);
//                         _tabIndex = DrawTabGUI(r, _tabArr, _tabIndex);
//                         EditorGUILayout.Space(20);
//
//                         SettingsObjectInfo objInfo = _objList[_tabIndex];
//                         DrawSettingsObj(objInfo);
//                     
//                         EditorGUILayout.EndVertical();
//                     }
//                 }
//                 else
//                 {
//                     for (int i = 0; i < _objList.Count; i++)
//                     {
//                         var objInfo = _objList[i];
//                         
//                         Rect r = EditorGUILayout.BeginVertical(SettingsFrameBox);
//                         EditorGUILayout.Space(20);
//                         DrawTabGUI(r, objInfo.TabArr, 0);
//                         DrawSettingsObj(objInfo);
//                         EditorGUILayout.EndVertical();
//
//                         if (i < _objList.Count - 1)
//                             EditorGUILayout.Space(10);
//                     }
//                 }
//                 
//                 OnAfterDraw();
//             // }
//         }
//
//         protected virtual void DrawSettingsObj(SettingsObjectInfo objInfo)
//         {
//             if (objInfo.SerializedObject != null && objInfo.SerializedObject.targetObject != null)
//             {
//                 objInfo.SerializedObject.Update();
//                 EditorGUI.BeginChangeCheck();
//
//                 OnBeforeDrawSettings(objInfo.Name);
//                     
//                 // EditorGUILayout.HelpBox($"{objInfo.ScriptableObj.GetType().Name}", MessageType.Info);
//                 foreach (FieldInfo fieldInfo in objInfo.FieldInfos)
//                 {
//                     SerializedProperty p = objInfo.SerializedObject.FindProperty(fieldInfo.Name);
//                     if (p == null) continue;
//                     OnDrawSettingsProperty(p);
//                 }
//                     
//                 OnAfterDrawSettings(objInfo.Name);
//                 
//                 if (EditorGUI.EndChangeCheck())
//                 {
//                     objInfo.SerializedObject.ApplyModifiedProperties();
//                     SaveData();
//                     OnSettingsChanged(objInfo.Name);
//                 }
//             }
//         }
//
//         protected virtual void OnfocusChanged(bool value)
//         {
//             if (value) LoadData();
//             else SaveData();
//         }
//
//         private void LoadData()
//         {
//             // Debug.Log($"LoadData: {GetType().Name}");
//             // return;
//             
//             _objList.Clear();
//             
//             var objs = LoadObjects();
//             if(objs == null) return;
//             foreach (var o in objs) _objList.Add(new SettingsObjectInfo(o));
//             _tabArr = _objList.Select(item => item.TabName).ToArray();
//         }
//         private void SaveData() => SaveObjects();
//
//         protected virtual void OnBeforeDraw() { }
//         protected virtual void OnAfterDraw() { }
//         protected virtual void OnBeforeDrawSettings(string settingsName) { }
//         protected virtual void OnAfterDrawSettings(string settingsName) { }
//         protected virtual void OnDrawSettingsProperty(SerializedProperty property)
//         {
//             EditorGUILayout.PropertyField(property);
//         }
//         protected virtual void OnSettingsChanged(string settingsName) { }
//
//         // protected abstract Type[] GetSettings();
//
//         protected abstract ScriptableObject[] LoadObjects();
//
//         protected virtual void SaveObjects()
//         {
//             // Debug.Log($"SaveObjects: {GetType().Name}");
//             // return;
//             
//             foreach (SettingsObjectInfo objectInfo in _objList)
//             {
//                 objectInfo.ScriptableObj.SaveEx();
//             }
//         }
//
//         protected IDisposable CreateSettingsWindowGUIScope()
//         {
//             var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
//             var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
//             return Activator.CreateInstance(type) as IDisposable;
//         }
//         
//         protected readonly GUIStyle SettingsFrameBox = new GUIStyle(GUIStyles.FrameBox) { padding = new RectOffset(1, 1, 1, 0) };
//         
//         protected int DrawTabGUI(Rect rect, string[] contents, int tabIndex)
//         {
//             GUIStyle buttonStyle = null;
//             for (int i = 0; i < contents.Length; i++)
//             {
//                 Rect buttonRect = GUIStyles.GetTabRect(rect, i, contents.Length, out buttonStyle);
//                 if (GUI.Toggle(buttonRect, i == tabIndex, contents[i], buttonStyle)) tabIndex = i;
//             }
//             return tabIndex;
//         }
//         
//         protected int DrawTabGUI(Rect rect, GUIContent[] contents, int tabIndex, System.Action<int> action = null)
//         {
//             GUIStyle buttonStyle = null;
//             for (int i = 0; i < contents.Length; i++)
//             {
//                 Rect buttonRect = GUIStyles.GetTabRect(rect, i, contents.Length, out buttonStyle);
//                 if (GUI.Toggle(buttonRect, i == tabIndex, contents[i], buttonStyle)) tabIndex = i;
//             }
//             return tabIndex;
//         }
//         
//         private const float LineThickness = 1f;
//         private static Color LineColor = Color.black;
//         protected void DrawLine()
//         {
//             Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth, LineThickness);
//             rect.x = 0; // 从左侧边缘开始
//             EditorGUI.DrawRect(rect, LineColor);
//         }
//
//     }
// }