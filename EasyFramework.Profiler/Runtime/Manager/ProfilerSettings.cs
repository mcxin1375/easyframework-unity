/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/25
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Profiler
{
    [ProjectSettings(ProjectSettingsAttribute.ETag.Resources)]
    public class ProfilerSettings : ProjectSettings<ProfilerSettings>
    {
        [Header("初始化")] 
        public bool autoInitialize;

        [Header("输入遮罩")]
        [Tooltip("阻止性能详情和错误面板上的鼠标、触摸点击穿透到下层 UI")]
        public bool blockUIInput = true;
        
        [Header("错误弹窗")]
        public bool errorGUIBehaviour = true;
        public Color errorBgColor = new Color(0, 0, 0, 0.8f);
        public Color errorFontColor = Color.red;
        public int errorFontSize = 30;
        
        [Header("性能分析")]
        public bool profilerGUIBehaviour = true;
        public Color recorderBgColor = new Color(0, 0, 0, 0.8f);
        public Color recorderFontColor = Color.white;
        public int recorderFontSize = 30;
        
        [Header("多镜头检测")]
        public bool multipleCameraDebugBehaviour;
    }
}
