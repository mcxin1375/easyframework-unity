/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework.Profiler
{
    public class ErrorGUIBehaviour : MonoBehaviour
    {
        private GUIContent _labelContent = new GUIContent();
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private Texture2D _bgTexture;
        private Texture2D _btnTexture;
        
        private Vector2 _scrollPos;
        private Vector2 _touchPos;
        private float _scrollStartY;
        
        private readonly Stack<Message> _logStack = new();
        

        void Awake()
        {
            if (_bgTexture == null)
            {
                _bgTexture = new Texture2D(1, 1);
                _bgTexture.SetPixel(0, 0, FProfiler.Settings.errorBgColor);
                _bgTexture.Apply();
            }
            if (_btnTexture == null)
            {
                _btnTexture = new Texture2D(1, 1);
                _btnTexture.SetPixel(0, 0, new Color(0.4f, 0.4f, 0.4f, 1));
                _btnTexture.Apply();
            }
            
            _labelStyle = new GUIStyle()
            {
                fontSize = FProfiler.Settings.errorFontSize,
                contentOffset = new Vector2(10, 10),
                wordWrap = true,
                normal = { background = _bgTexture, textColor = FProfiler.Settings.errorFontColor },
            };
            
            _buttonStyle = new GUIStyle
            {
                fontSize = FProfiler.Settings.errorFontSize,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = _btnTexture, textColor = Color.white }
            };
            
            F.InputManager.OnInputEvent += OnInputAction;
        }

        void OnDestroy()
        {
            F.InputManager.OnInputEvent -= OnInputAction;
        }

        private void OnGUI()
        {
            if (_logStack.Count == 0) return;

            if (!_logStack.TryPeek(out var logMessage)) return;
            
            _labelContent.text = logMessage.ToString();
            var labelHeight = _labelStyle.CalcHeight(_labelContent, Screen.width) + 100;
            if (labelHeight < Screen.height) labelHeight = Screen.height;
            var viewRect = new Rect(0, 0, Screen.width, Screen.height);
            var fullRect = new Rect(0, 0, Screen.width, labelHeight);
           
            _scrollPos = GUI.BeginScrollView(viewRect, _scrollPos, fullRect);
            GUI.Label(new Rect(0, 0, Screen.width, labelHeight), _labelContent, _labelStyle);
            GUI.EndScrollView();

            if (GUI.Button(new Rect(Screen.width - 200, 50, 130, 50), "Close", _buttonStyle))
                OnCloseLog();
            if (GUI.Button(new Rect(Screen.width - 350, 50, 130, 50), $"Clear({_logStack.Count})", _buttonStyle))
                OnClearLog();
            
            F.WindowManager.EventSystemEnabled = _logStack.Count == 0;
        }

        private void OnClearLog()
        {
            _logStack.Clear();
        }

        private void OnCloseLog()
        {
            if (_logStack.Count > 0) _logStack.TryPop(out _);
        }

        public void AddLog(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    _logStack.Push(new Message(condition, stackTrace, type));
                    break;
            }
        }

        private void OnInputAction(EInputType type, int value, Vector2 pos)
        {
            switch (type)
            {
                case EInputType.Down:
                    _touchPos = pos;
                    _scrollStartY = _scrollPos.y;
                    break;
                case EInputType.Hover:
                    _scrollPos.y = _scrollStartY + (pos - _touchPos).y * 3;
                    break;
                default:
                    break;
            }
        }
        
        private readonly struct Message
        {
            public readonly string Condition;
            public readonly string StackTrace;
            public readonly LogType LogType;
            public Message(string condition, string stackTrace, LogType logType)
            {
                Condition = condition;
                StackTrace = stackTrace;
                LogType = logType;
            }
            public override string ToString()
            {
                return $"LogType:{LogType}\n\ncondition:\n{Condition}\n\nstackTrace:\n{StackTrace}";
            }
        }
    }
}
