# F 总览

`F` 是 EasyFramework 的全局入口。业务通过 `F.xxx` 访问运行时模块；Controller 的进入和退出由业务 Controller 实例负责。

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

`autoInitialize` 默认关闭；开启后 Unity 回调会发起初始化，业务仍应调用 `await F.InitializeAsync()` 作为等待点。初始化成功后重复调用直接完成，初始化过程中其他调用等待同一状态。

当前异常路径没有将初始化状态复位；资源 Manifest 加载失败也可能仅记录错误后返回。初始化完成不能替代资源存在性检查，排查失败时应查看日志和发布输出，不能假设再次调用一定会重试。

## 入口索引

| 入口 | 职责 | 文档 |
| --- | --- | --- |
| `F.Settings` | 参数与运行路径 | [Settings](EasyFrameworkSettings.md) |
| `F.Behaviour` | Unity 生命周期宿主 | [Behaviour](FBehaviour.md) |
| `F.ControllerManager` | Controller 栈与状态查询 | [ControllerManager](ControllerManager.md) |
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
