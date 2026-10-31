# Profiler

Profiler 是包内独立模块，程序集为 `EasyFramework.Profiler`，命名空间为 `EasyFramework.Profiler`。它不通过 F.Profiler 访问，用于性能面板、错误显示、多摄像机检查和 Shader 变体错误通知。

## 配置与启动

项目设置由 ProfilerSettings 保存，Editor 入口为 Project Settings > EasyFramework Profiler。配置包括性能面板、错误面板的开关、颜色和字号，以及多摄像机检测开关。

当前 ProfilerManager 的运行时初始化回调直接创建单例，没有读取 ProfilerSettings.autoInitialize。不要以该字段判断模块是否会启动；性能面板默认开启，多摄像机检查默认关闭。

```csharp
using EasyFramework.Profiler;

public static class ProfilerExample
{
    public static void HidePanels()
    {
        ProfilerManager.Instance.ProfilerGUIBehaviour = false;
        ProfilerManager.Instance.ErrorGUIBehaviour = false;
        ProfilerManager.Instance.MultipleCameraDebugBehaviour = false;
    }
}
```

以上三个属性仅提供 setter。自定义 asmdef 需要引用 EasyFramework.Profiler。开关在运行时设置，默认值在 ProfilerSettings 中保存。

## 数据与事件边界

面板通过 ProfilerRecorder 收集帧率、内存和渲染统计，可用计数器随 Unity 版本、平台与构建方式变化，不能假设所有数值在 Player 中都有效。

OnShaderVariantError 参数为 ShaderVariantInfo。事件来自 Application.logMessageReceivedThreaded，回调可能发生在工作线程，不应直接操作 Unity 对象；需要更新 UI 时先调度回主线程。相同错误文本在管理器生命周期内只通知一次，结束使用时取消订阅。

源码：[ProfilerManager.cs](../../Module/Profiler/Runtime/Unity/ProfilerManager.cs)、[ProfilerSettings.cs](../../Module/Profiler/Runtime/Unity/ProfilerSettings.cs)、[ProfilerProvider.cs](../../Module/Profiler/Editor/ProfilerProvider.cs)。
