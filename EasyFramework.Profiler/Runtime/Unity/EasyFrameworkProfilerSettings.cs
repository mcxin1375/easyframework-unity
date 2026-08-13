/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/25
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Profiler
{
    // [EasyMain.EasyProjectSettings("ProjectSettings/EasyFramework/ProfilerSettings.asset")]
    public class EasyFrameworkProfilerSettings : ProjectSettingsAssetBundle<EasyFrameworkProfilerSettings>
    {
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