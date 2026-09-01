# F 总览

`F` 是 EasyFramework 的全局入口静态类。业务代码通过 `F.xxx` 访问框架模块，不直接依赖模块的具体实现类。

```csharp
await F.InitializeAsync();

await F.WindowManager.OpenAsync<LoginWindow>();
await F.ResLoader.CreateObjAsync("Cube");
await F.ControllerManager.EnterAsync<LoginController>();
```

## 初始化

框架初始化应在游戏进入业务状态前调用一次，并等待完成。当前项目的 `GameInitState` 正是在进入登录状态前执行 `await F.InitializeAsync()`；初始化完成后才进入 `GameLoginState`。

`F.InitializeAsync()` 会按依赖顺序执行：

1. 创建 `F.Settings`，配置调试器和日志级别。
2. 创建 `F.Behaviour`，建立跨场景生命周期宿主。
3. 创建 Window、Sprite、Shader、Scene、Audio 模块。
4. 等待 `F.DLCManager` 初始化。
5. 等待底层 AssetBundle 资源加载器初始化。

因此应在启动状态、启动器或等价的生命周期节点中执行 `await F.InitializeAsync()`，完成后再访问依赖资源的模块。`F.Initialize()` 只会发起初始化而不等待，不适合作为需要立即使用资源、窗口或 DLC 的唯一调用。

```csharp
private async ETask InitializeGameAsync()
{
    await F.InitializeAsync();
    // 初始化完成后再进入登录、加载主界面或创建业务资源。
    GLogic.FSM.Enter<GameLoginState>();
}
```

当 `Settings.autoInitialize` 为 `true` 时，框架还会通过 Unity 的运行时初始化回调自动发起初始化；业务流程仍应把 `F.InitializeAsync()` 作为等待点，避免与初始化并行产生竞态。多次并发等待会等待同一初始化状态，但业务侧仍应统一管理调用位置。

## 入口索引

| 入口 | 主要职责 | 详细文档 |
| --- | --- | --- |
| `F.Settings` | 项目运行参数与路径 | [EasyFrameworkSettings.md](EasyFrameworkSettings.md) |
| `F.Behaviour` | 跨场景宿主、帧循环与销毁 | [FBehaviour.md](FBehaviour.md) |
| `F.WorldManager` | World、Entity、System 生命周期 | [WorldManager.md](WorldManager.md) |
| `F.ControllerManager` | Controller 进入、退出和激活 | [ControllerManager.md](ControllerManager.md) |
| `F.Event` | 泛型事件注册与派发 | [Event.md](Event.md) |
| `F.ResLoader` | AssetBundle 与资源对象加载 | [ResLoader.md](ResLoader.md) |
| `F.DLCManager` | DLC 版本、文件和下载 | [DLCManager.md](DLCManager.md) |
| `F.HttpManager` | HTTP 文本请求与文件下载 | [HttpManager.md](HttpManager.md) |
| `F.WindowManager` | UI 窗口、层级和生命周期 | [WindowManager.md](WindowManager.md) |
| `F.SpriteLoader` | SpriteAtlas 与 Sprite | [SpriteLoader.md](SpriteLoader.md) |
| `F.ShaderLoader` | Shader 加载与查询 | [ShaderLoader.md](ShaderLoader.md) |
| `F.SceneLoader` | 场景加载、卸载和激活 | [SceneLoader.md](SceneLoader.md) |
| `F.InputManager` | 输入事件 | [InputManager.md](InputManager.md) |
| `F.PoolManager` | GameObject 对象池 | [PoolManager.md](PoolManager.md) |
| `F.AudioPlayer` | 音频、音乐和频道控制 | [AudioPlayer.md](AudioPlayer.md) |
| `F.HybridCLRManager` | HybridCLR 进入与程序集加载 | [HybridCLRManager.md](HybridCLRManager.md) |

`F.HybridCLRManager` 只有项目定义 `EF_HYBRIDCLR` 时存在；`F.MainResManager` 当前是注释代码，不属于有效入口。

源码：[F.cs](../../Runtime/F.cs)。
