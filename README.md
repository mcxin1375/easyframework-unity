# EasyFramework for Unity

EasyFramework 是面向 Unity 的模块化 C# 框架，围绕异步任务、业务流程、资源与 UI，提供从游戏运行到构建发布的一套开发工具。UPM 包名为 `com.cookie.easyframework`，当前版本为 `1.0.0`。

[核心能力](#核心能力) · [快速开始](#快速开始) · [文档索引](#文档索引) · [更新日志](CHANGELOG.md) · [许可证](LICENSE)

## 核心能力

### ETask · 面向 0 GC 的轻量异步

`ETask` / `ETask<T>` 以 0 GC 为优化方向，使用值类型、自定义异步方法构建器和池化 awaiter，减少异步任务的分配开销。支持时间与帧等待、条件等待，以及 Unity 异步操作适配，让异步流程保持直观：

```csharp
await ETask.DelaySeconds(1);
```

实际分配取决于等待的操作、续体、闭包及业务代码，并非所有调用都保证 0 GC；池化任务应由调用方消费一次。[了解 ETask](Documentation/Runtime/ETask.md)

### ResLoader · 加载资源只需资源名

初始化框架并配置资源后，传入资源名即可加载并实例化：

```csharp
var cube = await F.ResLoader.CreateObjAsync("Cube");
```

业务无需拼接资源路径、Bundle 名或哈希文件名。Player 使用构建清单定位资源并处理依赖；Editor 默认通过资源桥接加载，可不打包调试。长期持有资源时，可通过 `IResRequest` 管理持有与释放。[了解资源管理](Documentation/Runtime/ResLoader.md)

### PlayerBuilder · 一键构建发布

完成资源目录、应用设置、构建平台与场景配置后，执行：

```text
EasyFramework/Tools/PlayerBuilder - Execute
```

默认流程先构建 AssetBundle 和 DLC 版本，再同步应用设置并构建 Player。构建步骤可通过 `IToolEvent<T>` 扩展；使用 `DLC_StreamingAssets` 时需按项目配置资源复制。[了解 PlayerBuilder](Documentation/Editor/PlayerBuilder.md)

### 业务、界面与基础模块

- **Controller**：用进入、退出、激活和帧分发组织业务流程，支持组件与参数传递。
- **Window**：按类型管理窗口，支持分层、同步或异步打开、带参和生命周期回调；可结合 Roslyn 生成器辅助 UI 绑定。
- **常用模块**：事件、FSM、场景、输入、音频、对象池、Sprite 与 Shader。
- **Profiler**：独立程序集，提供性能与错误面板、多摄像机检查和 Shader 错误通知。
- **HybridCLR**：提供可选集成与构建入口，完整热更新流程需要项目配置和验证。

## 快速开始

### 1. 准备依赖并安装

`package.json` 声明 Newtonsoft.Json 和 Scriptable Build Pipeline 依赖；包本身未声明最低 Unity 版本，[配套示例工程](https://github.com/mcxin1375/easyframework-projects)使用 Unity 2022.3.62f3。

当前程序集还无条件引用 TextMesh Pro、Universal RP 和 HybridCLR；UI 功能需要 Unity UI。请先在目标工程中准备这些依赖，再通过 Unity Package Manager 的 **Add package from git URL** 安装：

```text
https://github.com/mcxin1375/easyframework-unity.git
```

HybridCLR 功能按需启用，但其程序集引用目前仍是包的编译前提。不使用这些集成的项目，需要在自己维护的包中处理相应程序集引用和代码依赖。完整步骤与限制见 [安装前提](Documentation/GettingStarted.md)。

使用自定义 asmdef 时，业务程序集引用 `EasyFramework`，Editor 工具程序集引用 `EasyFramework.Editor`；使用 Profiler 的程序集还需引用 `EasyFramework.Profiler`。

安装后，可在 `Package Manager > Easy Framework > Samples` 导入 **EasyFramework Samples**。示例是按 Runtime 和 Editor 分类的简短代码，不包含独立运行场景；资源、URL 和构建配置由接入工程提供。参见 [示例说明](Documentation/Examples.md)。

### 2. 初始化并进入业务

先在 `Project Settings > EasyFramework Tools > AssetBundleBuilder` 配置资源目录，默认目录为 `Assets/Res_DLC`。准备一个根节点为 `RectTransform` 的 `HelloWindow.prefab` 并放入构建目录，然后将以下 `Bootstrap` 挂到启动场景的 GameObject 上：

```csharp
using EasyFramework;
using UnityEngine;

public sealed class Bootstrap : MonoBehaviour
{
    private void Start() => StartGameAsync().Forget();

    private async ETask StartGameAsync()
    {
        await F.InitializeAsync();
        if (this == null) return;

        await GameController.Instance.EnterAsync();
    }
}

public sealed class GameController : Controller<GameController>
{
    protected override async ETask OnEnterAsync()
    {
        await F.WindowManager.OpenAsync<HelloWindow>(UILayer.HUD);
    }
}

public sealed class HelloWindow : Window
{
    protected override void OnOpen() => Debug.Log("Hello, EasyFramework!");
}
```

先等待 `F.InitializeAsync()`，再使用资源、窗口和 DLC。Controller 也可以通过 `F.ControllerManager` 的泛型接口进入和退出；异步打开的窗口应等待完成后再访问。

Editor 默认使用资源桥接，可在不构建 Bundle 的情况下调试。Player 需要先构建资源并准备 DLC。长期持有资源时，结束使用后应先销毁实例，再使用同一个 `IResRequest` 释放资源。窗口的关闭、销毁和资源释放约定见 [WindowManager](Documentation/Runtime/WindowManager.md)；完整示例见 [ResLoader](Documentation/Runtime/ResLoader.md)。

### 3. 配置资源与发布

1. 在 `AssetBundleBuilder` 的 Build Directories 中配置资源目录。普通文件按文件分别打包，以 `.ab` 结尾的目录整体打包；会跳过 `Editor`、`.meta` 和配置的过滤项。多个 `IAssetBundleBuilderSettings` 的配置会合并、去重。
2. 通过 `Create > EasyFramework > AppSettings` 创建应用配置，填写公司、产品、包标识、版本和 URL，并在 `Project Settings > EasyFramework` 中引用。
3. 检查当前 Build Target 和 Build Settings 中启用的启动场景。
4. 使用 `DLC_StreamingAssets` 时，在 DLCBuilder 中开启 `copyVersionToStreamingAssets`，或手动执行 `EasyFramework/Tools/DLCBuilder - CopyVersionToStreamingAssets`；`releaseVersion` 留空时使用最新资源版本。
5. 执行 `EasyFramework/Tools/PlayerBuilder - Execute`。

默认 `PlayerBuilder.assetBuilder` 和 `preSettingsEnabled` 均为 `true`，会先构建资源，再同步应用设置并构建 Player。也可在 Editor 程序集中调用：

```csharp
using EasyFramework.Editor;

public static class GameBuild
{
    public static void Execute() => PlayerBuilder.Instance.Execute();
}
```

需要分别执行资源和 Player 构建时，先关闭 `PlayerBuilder.assetBuilder`，再按 AssetBuilder、PlayerBuilder 的顺序执行。详细规则见 [AssetBundleBuilder](Documentation/Editor/AssetBundleBuilder.md)、[DLCBuilder](Documentation/Editor/DLCBuilder.md) 和 [PlayerBuilder](Documentation/Editor/PlayerBuilder.md)。

## 文档索引

从 [快速开始](Documentation/GettingStarted.md) 和 [F 总览](Documentation/Runtime/F.md) 进入，按需查看模块接口、生命周期与使用边界。

| 方向        | 文档                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| --------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 入门与示例     | [快速开始](Documentation/GettingStarted.md) · [示例说明](Documentation/Examples.md)                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| 框架基础      | [F](Documentation/Runtime/F.md) · [ETask](Documentation/Runtime/ETask.md) · [Settings](Documentation/Runtime/EasyFrameworkSettings.md) · [Behaviour](Documentation/Runtime/FBehaviour.md)                                                                                                                                                                                                                                                                                                                         |
| 业务组织      | [Controller](Documentation/Runtime/ControllerManager.md) · [同步事件](Documentation/Runtime/Event.md) · [异步事件](Documentation/Runtime/AsyncEvent.md)                                                                                                                                                                                                                                                                                                                                                                   |
| 资源与内容     | [ResLoader](Documentation/Runtime/ResLoader.md) · [DLCManager](Documentation/Runtime/DLCManager.md) · [HttpManager](Documentation/Runtime/HttpManager.md)                                                                                                                                                                                                                                                                                                                                                         |
| UI 与表现    | [WindowManager](Documentation/Runtime/WindowManager.md) · [SceneLoader](Documentation/Runtime/SceneLoader.md) · [SpriteLoader](Documentation/Runtime/SpriteLoader.md) · [ShaderLoader](Documentation/Runtime/ShaderLoader.md)                                                                                                                                                                                                                                                                                     |
| 常用模块      | [InputManager](Documentation/Runtime/InputManager.md) · [PoolManager](Documentation/Runtime/PoolManager.md) · [AudioPlayer](Documentation/Runtime/AudioPlayer.md)                                                                                                                                                                                                                                                                                                                                                 |
| 集成与诊断     | [HybridCLRManager](Documentation/Runtime/HybridCLRManager.md) · [Profiler](Documentation/Runtime/Profiler.md) · [ECS 当前边界](Documentation/Runtime/WorldManager.md)                                                                                                                                                                                                                                                                                                                                                 |
| Editor 工具 | [ToolBase](Documentation/Editor/ToolBase.md) · [AssetImporter](Documentation/Editor/AssetImporter.md) · [AssetCreator](Documentation/Editor/AssetCreator.md) · [AssetBuilder](Documentation/Editor/AssetBuilder.md) · [AssetBundleBuilder](Documentation/Editor/AssetBundleBuilder.md) · [HybridCLRBuilder](Documentation/Editor/HybridCLRBuilder.md) · [DLCBuilder](Documentation/Editor/DLCBuilder.md) · [PlayerBuilder](Documentation/Editor/PlayerBuilder.md) |

## 接入边界

- DLC 已提供基础版本和文件下载，**独立分包下载暂不可用**。
- HybridCLR 是可选集成。Player 侧部分下载、加载路径依赖项目配置或后续接入，不应视为安装后即可独立工作的完整热更新方案。
- ECS 类型仍保留，但当前没有公开的 `WorldManager` / `F.WorldManager`，`FWorld` 构造函数为 `internal`。业务接入前请先阅读 [ECS 文档](Documentation/Runtime/WorldManager.md)。
- Controller 支持管理器帧分发与 `ExitAllAsync()`；不同进入、退出入口的事件行为和异常处理见 [Controller 文档](Documentation/Runtime/ControllerManager.md)。

Profiler 的历史验证记录包括 Edit Mode 271/271、Play Mode 133/133，以及 Windows Mono Development / 正式包最小场景冒烟；这不代表完整业务 Player、Android / IL2CPP 或真机验收。验证条件见 [Profiler 文档](Documentation/Runtime/Profiler.md)。

## 包内结构

```text
EasyFramework/Runtime/   运行时核心与模块
EasyFramework/Editor/    编辑器工具与构建流程
EasyFramework.Profiler/  独立性能诊断模块
Documentation/Runtime/  Runtime 模块文档
Documentation/Editor/   Editor 工具文档
Samples~/Runtime/       Runtime 示例
Samples~/Editor/        Editor 示例
Tests/                  自动化测试
Plugins/                生成器等插件
```

## 许可证

基于 [MIT License](LICENSE) 开源。
