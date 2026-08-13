// using UnityEngine;
// using UnityEditor;
// using System;
// using System.Reflection;
// using System.Linq;
//
// namespace EasyFramework.Editor
// {
//     public interface IExcelViewer
//     {
//         object DrawObject { get; }
//         float RowHeight => 22f;
//         float ColWidth => 120f;
//         float RowSpacing => 2f;
//     }
//
//     public class ExcelViewerWindow : EditorWindow
//     {
//         private FieldInfo[] _arrayFields;
//         private string[] _options;
//         private int _selectedIndex;
//
//         private Vector2 _scrollPos;
//         private IExcelViewer _excelViewer;
//
//         [MenuItem("EasyFramework/Excel Viewer")]
//         public static void Open()
//         {
//             GetWindow<ExcelViewerWindow>("Excel Viewer");
//         }
//
//         private void OnEnable()
//         {
//             _excelViewer = EasyFrameworkReflection.CreateInstance<IExcelViewer>();
//             if (_excelViewer == null) return;
//             CacheArrayFields();
//         }
//
//         private void CacheArrayFields()
//         {
//             var loaderType = _excelViewer.DrawObject.GetType();
//
//             _arrayFields = loaderType
//                 .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
//                 .Where(f => f.FieldType.IsArray)
//                 .ToArray();
//
//             _options = _arrayFields.Select(f => f.FieldType.GetElementType().Name).ToArray();
//         }
//
//         private void OnGUI()
//         {
//             if (_excelViewer == null || _arrayFields == null || _arrayFields.Length == 0)
//             {
//                 EditorGUILayout.HelpBox("未找到任何数组字段", MessageType.Warning);
//                 return;
//             }
//
//             // 下拉选择数据类型
//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.LabelField("数据类型", GUILayout.Width(70));
//             _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _options);
//             EditorGUILayout.EndHorizontal();
//
//             var field = _arrayFields[_selectedIndex];
//             var array = field.GetValue(_excelViewer.DrawObject) as Array;
//
//             if (array == null || array.Length == 0)
//             {
//                 EditorGUILayout.HelpBox("该数组为空", MessageType.Info);
//                 return;
//             }
//
//             DrawTable(array);
//         }
//
//         private void DrawTable(Array array)
//         {
//             float rowHeight = _excelViewer.RowHeight;
//             float colWidth = _excelViewer.ColWidth;
//             float rowSpacing = _excelViewer.RowSpacing;
//
//             Type elementType = array.GetType().GetElementType();
//             var fields = elementType.GetFields(BindingFlags.Instance | BindingFlags.Public);
//             float tableWidth = fields.Length * colWidth;
//             float tableHeight = array.Length * (rowHeight + rowSpacing);
//
//             // === 冻结表头 ===
//             Rect headerRect = GUILayoutUtility.GetRect(position.width, rowHeight);
//             GUI.Box(headerRect, GUIContent.none);
//             for (int i = 0; i < fields.Length; i++)
//             {
//                 GUI.Label(new Rect(headerRect.x + i * colWidth, headerRect.y, colWidth, rowHeight),
//                     fields[i].Name, EditorStyles.boldLabel);
//             }
//
//             // === 滚动区域 ===
//             Rect scrollViewRect = GUILayoutUtility.GetRect(position.width, position.height - headerRect.height - 20);
//             _scrollPos = GUI.BeginScrollView(scrollViewRect, _scrollPos, new Rect(0, 0, tableWidth, tableHeight), true, true);
//
//             int firstIndex = Mathf.FloorToInt(_scrollPos.y / (rowHeight + rowSpacing));
//             int visibleCount = Mathf.CeilToInt(scrollViewRect.height / (rowHeight + rowSpacing)) + 1;
//             int lastIndex = Mathf.Min(array.Length, firstIndex + visibleCount);
//
//             for (int i = firstIndex; i < lastIndex; i++)
//             {
//                 var item = array.GetValue(i);
//                 if (item == null) continue;
//
//                 float y = i * (rowHeight + rowSpacing);
//                 Rect rowRect = new Rect(0, y, tableWidth, rowHeight);
//                 GUI.Box(rowRect, GUIContent.none);
//
//                 for (int c = 0; c < fields.Length; c++)
//                 {
//                     var value = fields[c].GetValue(item);
//                     string display = value?.ToString() ?? "null";
//                     Rect cellRect = new Rect(c * colWidth, y, colWidth, rowHeight);
//
//                     if (GUI.Button(cellRect, display))
//                     {
//                         Debug.Log($"[{elementType.Name}.{fields[c].Name}] Row {i} = {display}");
//                     }
//                 }
//             }
//
//             GUI.EndScrollView();
//         }
//     }
// }
