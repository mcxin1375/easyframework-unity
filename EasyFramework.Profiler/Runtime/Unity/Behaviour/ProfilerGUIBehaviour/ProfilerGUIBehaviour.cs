/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/25
// describe:
//----------------------------------------------------------------*/

using System.Text;
using UnityEngine;

namespace EasyFramework.Profiler
{
    public class ProfilerGUIBehaviour : MonoBehaviour
    {
        public readonly FPSEx FPSEx = new();
        public readonly ProfilerRecorderEx ProfilerRecorderEx = new();
        public readonly ProfilerDeviceEx ProfilerDeviceEx = new();
        
        private GUIStyle _detailsStyle;
        private GUIStyle _headerStyle;
        private Vector2 _scrollPosition;

        private const float UpdateInterval = 0.5f;
        
        private string _headerStr;
        private string _memoryStr;
        private string _renderStr;
        private string _deviceStr;
        private float _time;

        private bool _drawDetails;
        private Texture2D _bgTexture;
        
        void Awake()
        {
            if (_bgTexture == null)
            {
                _bgTexture = new Texture2D(1, 1);
                _bgTexture.SetPixel(0, 0, FProfiler.Settings.recorderBgColor);
                _bgTexture.Apply();
            }

            _headerStyle = new GUIStyle()
            {
                fontSize = FProfiler.Settings.recorderFontSize,
                alignment = TextAnchor.MiddleLeft,
                normal = { background = _bgTexture, textColor = FProfiler.Settings.recorderFontColor },
            };
            _detailsStyle = new GUIStyle
            {
                fontSize = FProfiler.Settings.recorderFontSize,
                alignment = TextAnchor.UpperLeft,
                normal = { background = _bgTexture, textColor = FProfiler.Settings.recorderFontColor },
            };
            
            ProfilerDeviceEx.AwakeTime = Time.realtimeSinceStartup;
            ProfilerDeviceEx.StartBatteryLevel = SystemInfo.batteryLevel < 0 ? 100 : (int)(SystemInfo.batteryLevel * 100);
        }

        private void OnDestroy()
        {
            ProfilerRecorderEx.Dispose();
        }

        void Update()
        {
            FPSEx.OnUpdate();
            ProfilerRecorderEx.OnUpdate();
            
            if (Time.time > _time)
            {
                _time = Time.time + UpdateInterval;

                StringBuilder sb = new StringBuilder();
                sb.Append($"FPS: <color=yellow>{FPSEx.FPS}</color>");
                sb.Append($" M(U/R): {ProfilerRecorderEx.TotalUsedMemory.FormatByte()}/{ProfilerRecorderEx.TotalReservedMemory.FormatByte()}");
                sb.Append($" SC: {ProfilerRecorderEx.SetPassCallsCount.FormatNumber()}");
                sb.Append($" DC: {ProfilerRecorderEx.DrawCallsCount.FormatNumber()}");
                sb.Append($" B: {ProfilerRecorderEx.BatchesCount.FormatNumber()}");
                sb.Append($" T: {ProfilerRecorderEx.TrianglesCount.FormatNumber()}");
                sb.Append($" V: {ProfilerRecorderEx.VerticesCount.FormatNumber()}");
                // sb.Append($" Main: {ProfilerRecorderEx.MainThread/1000000:N1}ms");
                
                _headerStr = sb.ToString();
                _memoryStr = ProfilerRecorderEx.MemoryToString();
                _renderStr = ProfilerRecorderEx.RenderToString();
                _deviceStr = ProfilerDeviceEx.DeviceToString();
            }
        }

        private GUIContent _labelContent1 = new();
        private GUIContent _labelContent2 = new();
        private GUIContent _labelContent3 = new();
        
        void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, Screen.width, 30), _headerStr, _headerStyle))
            {
                _drawDetails = !_drawDetails;
                F.WindowManager.EventSystemEnabled = !_drawDetails;
            }

            if (_drawDetails)
            {
                _labelContent1.text = _memoryStr;
                _labelContent2.text = _renderStr;
                _labelContent3.text = _deviceStr;
            
                float contentHeight1 = _detailsStyle.CalcHeight(_labelContent1, Screen.width / 2f);
                float contentHeight2 = _detailsStyle.CalcHeight(_labelContent2, Screen.width / 2f);
                var contentHeight = Mathf.Max(contentHeight1, contentHeight2);
                GUI.Label(new Rect(0, 31, Screen.width / 2f, contentHeight), _labelContent1, _detailsStyle);
                GUI.Label(new Rect(Screen.width / 2f, 31, Screen.width / 2f, contentHeight), _labelContent2, _detailsStyle);
                GUI.Label(new Rect(0, 31 + contentHeight, Screen.width, Screen.height - 31 - contentHeight), _labelContent3, _detailsStyle);
            }
        }
    }
}