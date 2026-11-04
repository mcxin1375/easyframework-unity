# F 总览

`F` 是 EasyFramework 的全局入口。业务通过 `F.xxx` 访问运行时模块；Controller 的实例和进入退出时机由 `F.ControllerManager` 统一管理，业务可直接通过 X.Instance.EnterAsync(...) 跳转。

```csharp
using EasyFramework;

public static class GameBootstrap
{
    public static async ETask StartAsync()
    {
        await F.InitializeAsync();
        await GameController.Instance.EnterAsync();
    }
}

public sealed class GameController : Controller<GameController>
{
    protected override void OnEnter() => FDebug.Log("Game entered");
}
```

## 初始化

在进入业务流程前等待 `F.InitializeAsync()`，再使用资源、窗口或 DLC。当前源码没有 `F.Initialize()`。

初始化依次创建配置并设置日志级别，创建 Window、Sprite、Shader、Scene、Audio 模块，再等待 DLCManager 和 AssetBundleLoader 初始化。`F.Behaviour` 由使用它的模块按需创建，不是这里显式创建的独立步骤。

`autoInitialize` 默认关闭；开启后 Unity 回调会发起初始化，业务仍可调用 `await F.InitializeAsync()` 作为等待点。初始化只使用 ETask，每次调用返回各自独立、单次消费的 ETask；成功后重复调用直接完成。请在 Unity 主线程调用初始化入口。

初始化直接使用静态 EState 的 None、Initializing、Initialized、Exception 状态，不创建 Initialization 对象。首个调用在 try/catch 中执行初始化；失败时记录原异常、进入 Exception 状态并重新抛出。其他调用在 Initializing 时通过各自的 ETask.WaitUntil 等待状态变化；恢复时若状态为 Exception，则抛出新的通用异常 `Exception("F.InitializeAsync Error!")`，原始原因查看初始化日志。

失败后下一次调用立即重新尝试初始化。当前实现没有保存异常、等待者计数或轮次隔离；若重试先于旧等待者恢复，旧等待者会继续依据当前全局状态等待或完成，不保证收到先前那轮失败，也不保证各调用收到相同异常实例。DLC 版本或清单失败、资源 Manifest 无效均向上传播。AssetBundleLoader 直接清空并填充内部资源字典，全部条目处理成功后才设置 Manifest；中途失败可能留下部分字典条目，重试时重新清空并填充。失败前已经创建的模块保留，重试复用这些模块；初始化成功也不能替代单个资源存在性检查。

## 入口索引

| 入口 | 职责 | 文档 |
| --- | --- | --- |
| `F.Settings` | 参数与运行路径 | [Settings](EasyFrameworkSettings.md) |
| `F.Behaviour` | Unity 生命周期宿主 | [Behaviour](FBehaviour.md) |
| `F.ControllerManager` | Controller 实例、进入退出、栈与帧分发 | [ControllerManager](ControllerManager.md) |
| `F.Event` | 同步派发与异步逐个等待事件 | [Event](Event.md) / [异步事件](AsyncEvent.md) |
| `F.ResLoader` | 资源加载与持有关系 | [ResLoader](ResLoader.md) |
| `F.DLCManager` | 版本与文件下载 | [DLCManager](DLCManager.md) |
| `F.HttpManager` | HTTP 文本与文件下载 | [HttpManager](HttpManager.md) |
| `F.WindowManager` | 窗口、层级与生命周期 | [WindowManager](WindowManager.md) |
| `F.SpriteLoader` | SpriteAtlas 与 Sprite | [SpriteLoader](SpriteLoader.md) |
| `F.ShaderLoader` | Shader 资源 | [ShaderLoader](ShaderLoader.md) |
| `F.SceneLoader` | 场景管理 | [SceneLoader](SceneLoader.md) |
| `F.InputManager` | 旧版 Unity Input 输入事件 | [InputManager](InputManager.md) |
| `F.PoolManager` | GameObject 对象池 | [PoolManager](PoolManager.md) |
| `F.AudioPlayer` | 音效、音乐与频道 | [AudioPlayer](AudioPlayer.md) |
| `F.HybridCLRManager` | 可选热更新接入，仅 EF_HYBRIDCLR 下存在 | [HybridCLRManager](HybridCLRManager.md) |

ETask 是独立类型，见 [ETask](ETask.md)。Profiler 使用独立程序集和命名空间，见 [Profiler](Profiler.md)。当前没有 `F.WorldManager`，ECS 的可用范围见 [ECS](WorldManager.md)。

完整流程见 [快速开始](../GettingStarted.md)，可导入代码见 [基础示例](../Examples.md)。

源码：[F.cs](../../Runtime/F.cs)。
