# Profiler

Profiler 是包内独立模块，程序集为 `EasyFramework.Profiler`，命名空间为 `EasyFramework.Profiler`。它不通过 F.Profiler 访问，用于性能面板、错误显示、多摄像机检查和 Shader 变体错误通知。

## 配置与启动

项目设置由 ProfilerSettings 保存，Editor 入口为 Project Settings > EasyFramework Profiler。ProfilerManager 是唯一运行时全局单例和组件宿主，继承框架已有的 SingletonBehaviour；性能面板、错误面板、多摄像机检查及输入遮罩均为普通 MonoBehaviour，由 Manager 按设置创建与协调。运行时功能通过 ProfilerManager.Instance 访问，不再直接创建 ProfilerBehaviour 或遮罩单例。配置包括模块开关、输入遮罩开关、颜色和字号。

`ProfilerSettings.autoInitialize` 默认关闭。运行时初始化回调仅在该值为 true 时启动管理器；为 false 时不会自动创建 Profiler 宿主、面板、Recorder 或日志订阅。性能面板的配置默认开启，多摄像机检查默认关闭，这些配置只在启动模块后生效。发布前应在 Project Settings 中明确设置是否自动启动诊断组件。

关闭自动初始化后，仍可在 Unity 主线程显式启动：

```csharp
using EasyFramework.Profiler;

ProfilerManager.CreateInstance();
```

访问 `ProfilerManager.Instance` 也会显式启动模块；开关仅控制自动入口。重复启动复用同一管理器与宿主。运行中修改 autoInitialize 不会停止已经启动的模块。字段默认值不覆盖项目已保存的设置；当前验证工程原有 autoInitialize=true 保持不变。

## 运行时模块与开关

自定义 asmdef 需要引用 EasyFramework.Profiler。以下属性均在 Unity 主线程通过 `ProfilerManager.Instance` 读取或设置：

| Manager 属性 | Settings 字段 | 字段默认值 | 行为 |
| --- | --- | --- | --- |
| `ProfilerGUIBehaviour` | `profilerGUIBehaviour` | true | 开启时添加性能面板，关闭时移除并释放 Recorder |
| `ErrorGUIBehaviour` | `errorGUIBehaviour` | true | 开启时按需添加错误面板并接收新错误；关闭接收后保留已有面板与日志 |
| `MultipleCameraDebugBehaviour` | `multipleCameraDebugBehaviour` | false | 按开关添加或移除多摄像机检查 |
| `BlockUIInput` | `blockUIInput` | true | 按当前面板持有状态启停透明遮罩的射线拦截，保留缓存对象 |

Manager 每帧检查 Settings 变化；通过上述属性写入时立即协调设置和模块。组件移除遵循 Unity 的帧末销毁时机，同帧关闭再开启后，Manager 会在后续更新中补回缺失组件。错误面板继续使用原 Close / Clear 关闭或清空已有日志，关闭错误接收开关不等于隐藏面板。

```csharp
using EasyFramework.Profiler;

public static class ProfilerExample
{
    public static void StopModulesAndErrorCapture()
    {
        var profiler = ProfilerManager.Instance;
        profiler.ProfilerGUIBehaviour = false;
        profiler.MultipleCameraDebugBehaviour = false;
        profiler.ErrorGUIBehaviour = false; // 停止新错误接收，已有日志仍由 Close / Clear 处理。
    }
}
```

## 输入遮罩

性能详情展开或错误面板存在待显示日志时，Profiler 内部共用一个全屏透明 UGUI 射线遮罩，阻止新的鼠标、触摸点击穿透到下层 UI。`ProfilerSettings.blockUIInput` 默认开启，也可通过 `ProfilerManager.Instance.BlockUIInput` 修改；关闭后遮罩停止拦截，重新开启时按当前面板状态恢复。普通 `ProfilerInputBlocker` 组件由 Manager 按需添加，缓存 Canvas 和全屏透明 Image，只切换 Image 的 `raycastTarget`，不反复创建销毁。折叠详情、关闭最后一条错误、Clear、禁用或销毁面板时只移除自身的遮罩持有；另一面板仍显示时继续拦截。

面板不访问 WindowManager，也不修改 EventSystem 的开关，业务原有的启用或禁用状态继续生效。错误面板保留原有日志栈、滚动、Close / Clear 及 IMGUI 显示，原来的 EventSystem 开关写入由透明射线遮罩替代。此遮罩只拦截新的指针射线，不阻断原始 Input 读取、键盘或手柄导航，也不取消已经开始的拖拽。

## 数据与事件边界

面板通过 ProfilerRecorder 收集帧率、内存和渲染统计，可用计数器随 Unity 版本、平台与构建方式变化，不能假设所有数值在 Player 中都有效。有效计数器的 0 会正常更新；不可用计数器清除旧数值，文本显示 `N/A`。直接读取 long 属性时，可结合 `ProfilerRecorderEx.IsAvailable(nameof(ProfilerRecorderEx.DrawCallsCount))` 判断可用性；`FormatNumber` / `FormatByte` 也按属性名区分 0 和不可用。详情及头部按原有 0.5 秒间隔刷新文本。

`Application.logMessageReceivedThreaded` 回调只向并发队列写入错误、断言和异常日志。ProfilerManager.Update 在 Unity 主线程消费队列、解析和去重 Shader 错误、更新错误面板，再派发 `OnShaderVariantError`；日志到界面与事件之间有帧延迟。原 ProfilerBehaviour 单例宿主已移除，功能组件只通过 Manager 协调。

OnShaderVariantError 参数为 ShaderVariantInfo，订阅者在主线程执行。相同 Shader 错误文本在管理器生命周期内只处理一次；普通错误不去重。无法解析的 Shader 文本作为普通错误显示，订阅者异常被记录，后续日志和订阅者仍可处理。结束使用时取消订阅。`ProfilerManager.DestroyInstance()` 按 Unity 的延迟销毁规则结束 Manager 宿主、托管组件及缓存遮罩；Manager.OnDestroy 取消日志订阅并丢弃其待处理队列，之后晚到的旧回调不会通知新管理器。

## 验证范围

2026-11-05 在当前 EasyFramework.Unity2022 工程使用 Unity 2022.3.62f3 验证：完整 Edit Mode 269/269、Play Mode 130/130，均为 0 失败、0 跳过。覆盖自动/显式启动、Settings 切换与同帧关闭再开启、800 条多线程 Unity 错误日志、共享遮罩缓存及合成鼠标/触摸指针、真实 Recorder 非零→零→非零及面板文本同步。

射线测试使用 EventSystem.RaycastAll 和合成 PointerEventData，不代表物理鼠标、真机触摸或 IMGUI 外观验收。未执行 Player、Android 或 IL2CPP 构建；完整进度与剩余 R09、R14 范围见 [REVIEW](../../REVIEW.md)。

源码：[ProfilerManager.cs](../../Module/Profiler/Runtime/Unity/ProfilerManager.cs)、[ProfilerSettings.cs](../../Module/Profiler/Runtime/Unity/ProfilerSettings.cs)、[ProfilerProvider.cs](../../Module/Profiler/Editor/ProfilerProvider.cs)。
