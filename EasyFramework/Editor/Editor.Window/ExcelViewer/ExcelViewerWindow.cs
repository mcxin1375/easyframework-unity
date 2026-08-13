using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace EasyFramework.Editor
{
    public interface IExcelViewer
    {
        object DrawObject { get; }
        float RowHeight => 22f;
        float ColWidth => 120f;
        float RowSpacing => 1f;

        // void ReLoad();
    }

    public class ExcelViewerWindow : EditorWindow
    {
        private FieldInfo[] _arrayFields;
        private string[] _options;
        private int _selectedIndex;

        private Vector2 _scrollPos;
        private IExcelViewer _excelViewer;
        private string _searchText = string.Empty; // 搜索输入内容

        private object _selectedObject; // 记录高亮的行索引

        public static void Open()
        {
            GetWindow<ExcelViewerWindow>("Excel Viewer");
        }

        private void OnEnable()
        {
            _excelViewer = EasyFrameworkReflection.CreateInstance<IExcelViewer>();
            if (_excelViewer == null) return;
            CacheArrayFields();
        }

        private void CacheArrayFields()
        {
            var loaderType = _excelViewer.DrawObject.GetType();

            _arrayFields = loaderType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.FieldType.IsArray)
                .ToArray();

            _options = _arrayFields.Select(f => f.FieldType.GetElementType().Name).ToArray();
        }

        private void OnGUI()
        {
            if (_excelViewer == null) return;
            if (_arrayFields == null || _arrayFields.Length == 0)
            {
                EditorGUILayout.HelpBox("未找到任何数组字段", MessageType.Warning);
                return;
            }

            Array array = null;

            EditorGUILayout.BeginHorizontal();
            // 下拉菜单选择类型
            _selectedIndex = EditorGUILayout.Popup("数据类型", _selectedIndex, _options);
            var field = _arrayFields[_selectedIndex];
            array = field.GetValue(_excelViewer.DrawObject) as Array;
            EditorGUILayout.LabelField($"数据 ({array?.Length ?? 0} 条)", EditorStyles.boldLabel);

            _searchText = EditorGUILayout.TextField(_searchText, GUILayout.Width(200));

            EditorGUILayout.EndHorizontal();

            if (array != null)
                DrawTable(array);
        }

        private void DrawTable(Array array)
        {
            Type elementType = array.GetType().GetElementType();
            var fields = elementType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            if (fields.Length == 0) return;

            // ==== 搜索过滤 ====
            List<object> filteredList = new List<object>();
            foreach (var obj in array)
            {
                if (obj == null) continue;

                var firstField = fields[0];
                var firstValue = firstField.GetValue(obj)?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(_searchText) || 
                    firstValue.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                {
                    filteredList.Add(obj);
                }
            }

            EditorGUILayout.Space();

            float rowHeight = _excelViewer.RowHeight;
            float colWidth = _excelViewer.ColWidth;
            float rowSpacing = _excelViewer.RowSpacing;
            float tableWidth = fields.Length * colWidth;
            float tableHeight = filteredList.Count * (rowHeight + rowSpacing);

            // 表头
            Rect headerRect = GUILayoutUtility.GetRect(position.width, rowHeight);
            GUI.Box(headerRect, GUIContent.none);

            for (int i = 0; i < fields.Length; i++)
            {
                GUI.Label(new Rect(headerRect.x + i * colWidth - _scrollPos.x, headerRect.y, colWidth, rowHeight),
                    fields[i].Name, EditorStyles.boldLabel);
            }

            // 数据
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            Rect fullRect = GUILayoutUtility.GetRect(tableWidth, tableHeight);

            int firstIndex = Mathf.FloorToInt(_scrollPos.y / (rowHeight + rowSpacing));
            int visibleCount = Mathf.CeilToInt(position.height / (rowHeight + rowSpacing));
            int lastIndex = Mathf.Min(filteredList.Count, firstIndex + visibleCount);

            for (int i = firstIndex; i < lastIndex; i++)
            {
                var item = filteredList[i];
                if (item == null) continue;

                float rowY = fullRect.y + i * (rowHeight + rowSpacing);
                Rect rowRect = new Rect(fullRect.x, rowY, tableWidth, rowHeight);

                GUI.Box(rowRect, GUIContent.none);

                // === 设置按钮颜色 ===
                Color prevColor = GUI.backgroundColor;

                if (item == _selectedObject)
                {
                    GUI.backgroundColor = new Color(0.3f, 0.6f, 1f); // 高亮蓝色
                }
                else if (i % 2 == 0)
                {
                    GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f); // 偶数行浅灰
                }
                else
                {
                    GUI.backgroundColor = Color.white; // 奇数行白色
                }
                
                float x = rowRect.x;
                foreach (var f in fields)
                {
                    var value = f.GetValue(item);
                    string display = value?.ToString() ?? "null";

                    if (GUI.Button(new Rect(x, rowRect.y, colWidth, rowHeight), display))
                    {
                        _selectedObject = item; // 更新选中行
                        Debug.Log($"[{elementType.Name}.{f.Name}] Row {i} = {display}");
                        Repaint();
                    }

                    x += colWidth;
                }
                GUI.backgroundColor = prevColor; // 恢复颜色
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
