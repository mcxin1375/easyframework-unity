using System;
using EasyFramework.Profiler;

namespace EasyFramework.Samples.Modules
{
    // 引用 EasyFramework.Profiler；先在 Project Settings 配置诊断开关。
    // 只在业务显式调用时启动，不销毁项目共享的管理器。
    public static class ProfilerExample
    {
        private static ProfilerManager _profiler;

        public static void Start()
        {
            _profiler = ProfilerManager.Instance;
            _profiler.ProfilerGUIBehaviour = true;
        }

        public static void Hide()
        {
            if (_profiler != null) _profiler.ProfilerGUIBehaviour = false;
        }

        // 修改全局模块开关；停止接收错误不会清空已有错误面板。
        public static void Configure(bool performance, bool captureErrors, bool cameraDebug, bool blockUiInput)
        {
            _profiler = ProfilerManager.Instance;
            _profiler.ProfilerGUIBehaviour = performance;
            _profiler.ErrorGUIBehaviour = captureErrors;
            _profiler.MultipleCameraDebugBehaviour = cameraDebug;
            _profiler.BlockUIInput = blockUiInput;
        }

        // Shader 错误事件在主线程派发，监听结束后用同一委托解绑。
        public static void Subscribe(Action<ShaderVariantInfo> handler)
        {
            _profiler = ProfilerManager.Instance;
            _profiler.OnShaderVariantError += handler;
        }

        public static void Unsubscribe(Action<ShaderVariantInfo> handler)
        {
            if (_profiler != null) _profiler.OnShaderVariantError -= handler;
        }

        // 读取已启动面板的采样；不可用计数器显示 N/A，避免把不可用误认为零。
        public static void ReadCounters()
        {
            var panel = _profiler != null ? _profiler.GetComponent<ProfilerGUIBehaviour>() : null;
            if (panel == null) return;
            var counters = panel.ProfilerRecorderEx;
            bool drawCallsAvailable = counters.IsAvailable(nameof(ProfilerRecorderEx.DrawCallsCount));
            FDebug.Log($"内存：{counters.FormatByte(nameof(ProfilerRecorderEx.TotalUsedMemory))}，" +
                $"Draw Calls：{counters.FormatNumber(nameof(ProfilerRecorderEx.DrawCallsCount))}，可用：{drawCallsAvailable}");
        }

        // 只有诊断宿主的统一拥有者可以销毁它，其他消费者只停止自己的订阅。
        public static void DestroyOwnedHost()
        {
            ProfilerManager.DestroyInstance();
            _profiler = null;
        }
    }
}
