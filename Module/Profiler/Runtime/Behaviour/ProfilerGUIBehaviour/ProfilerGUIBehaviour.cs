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
        
        private ProfilerSettings Settings => ProfilerSettings.Instance;
        
        void Awake()
        {
            if (_bgTexture == null)
            {
                _bgTexture = new Texture2D(1, 1);
                _bgTexture.SetPixel(0, 0, Settings.recorderBgColor);
                _bgTexture.Apply();
            }

            _headerStyle = new GUIStyle()
            {
                fontSize = Settings.recorderFontSize,
                alignment = TextAnchor.MiddleLeft,
                normal = { background = _bgTexture, textColor = Settings.recorderFontColor },
            };
            _detailsStyle = new GUIStyle
            {
                fontSize = Settings.recorderFontSize,
                alignment = TextAnchor.UpperLeft,
                normal = { background = _bgTexture, textColor = Settings.recorderFontColor },
            };
            
            ProfilerDeviceEx.AwakeTime = Time.realtimeSinceStartup;
            ProfilerDeviceEx.StartBatteryLevel = SystemInfo.batteryLevel < 0 ? 100 : (int)(SystemInfo.batteryLevel * 100);
        }

        private void OnEnable() => UpdateInputBlocker();

        private void OnDisable() => SetInputBlocked(false);

        private void OnDestroy()
        {
            SetInputBlocked(false);
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
                sb.Append($" M(U/R): {ProfilerRecorderEx.FormatByte(nameof(ProfilerRecorderEx.TotalUsedMemory))}/{ProfilerRecorderEx.FormatByte(nameof(ProfilerRecorderEx.TotalReservedMemory))}");
                sb.Append($" SC: {ProfilerRecorderEx.FormatNumber(nameof(ProfilerRecorderEx.SetPassCallsCount))}");
                sb.Append($" DC: {ProfilerRecorderEx.FormatNumber(nameof(ProfilerRecorderEx.DrawCallsCount))}");
                sb.Append($" B: {ProfilerRecorderEx.FormatNumber(nameof(ProfilerRecorderEx.BatchesCount))}");
                sb.Append($" T: {ProfilerRecorderEx.FormatNumber(nameof(ProfilerRecorderEx.TrianglesCount))}");
                sb.Append($" V: {ProfilerRecorderEx.FormatNumber(nameof(ProfilerRecorderEx.VerticesCount))}");
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

        private void SetDetailsVisible(bool value)
        {
            _drawDetails = value;
            UpdateInputBlocker();
        }

        private void UpdateInputBlocker() => SetInputBlocked(_drawDetails && isActiveAndEnabled);

        private void SetInputBlocked(bool blocked)
        {
            if (ProfilerManager.HasInstance()) ProfilerManager.Instance.SetInputBlocked(this, blocked);
        }
        
        void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, Screen.width, 30), _headerStr, _headerStyle))
            {
                SetDetailsVisible(!_drawDetails);
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
