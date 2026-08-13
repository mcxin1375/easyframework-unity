using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class GUIStyles
    {

        internal const int kTabButtonHeight = 22;
        static GUIStyle s_TabOnlyOne;
        static GUIStyle s_TabFirst;
        static GUIStyle s_TabMiddle;
        static GUIStyle s_TabLast;

        public static GUIStyle FrameBox => GetStyle("FrameBox");
        
        public static Rect GetTabRect(Rect rect, int tabIndex, int tabCount, out GUIStyle tabStyle)
        {
            if (s_TabOnlyOne == null)
            {
                s_TabOnlyOne = "Tab onlyOne";
                s_TabFirst = "Tab first";
                s_TabMiddle = "Tab middle";
                s_TabLast = "Tab last";
            }

            tabStyle = s_TabMiddle;

            if (tabCount == 1)
            {
                tabStyle = s_TabOnlyOne;
            }
            else if (tabIndex == 0)
            {
                tabStyle = s_TabFirst;
            }
            else if (tabIndex == (tabCount - 1))
            {
                tabStyle = s_TabLast;
            }

            float tabWidth = rect.width / tabCount;
            int left = Mathf.RoundToInt(tabIndex * tabWidth);
            int right = Mathf.RoundToInt((tabIndex + 1) * tabWidth);
            return new Rect(rect.x + left, rect.y, right - left, kTabButtonHeight);
        }
        
        static GUIStyle GetStyle(string styleName)
        {
            GUIStyle s = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            // GUIStyle s = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (s == null)
            {
                Debug.LogError("Missing built-in guistyle " + styleName);
            }
            return s;
        }
        
        public static readonly GUIStyle MainStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold, // 主项加粗
            // normal = { textColor = new Color(0.2f, 0.5f, 1f) } // 主项蓝色
        };
        public static readonly GUIStyle DependencyStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } // 子项灰色
        };

        public const string MainPrefix = "• ";
        public const string DependencyPrefix = "  └ ";
    }
}