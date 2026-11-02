# EasyFramework for Unity

EasyFramework 是面向 Unity 项目的轻量级 C# 框架，核心思想：极简、易用。当前 UPM 包名为 `com.cookie.easyframework`，版本为 `0.1.1`。

## 安装与示例

在 Unity Package Manager 的 **Add package from git URL** 中输入：

```text
https://github.com/mcxin1375/easyframework-unity.git
```

package.json 声明 Newtonsoft.Json 与 Scriptable Build Pipeline 依赖，未声明最低 Unity 版本。当前 asmdef 还无条件引用 TMP、URP 与 HybridCLR 程序集，UI 需要 Unity UI；纯净工程仅添加 Git URL 可能出现缺失引用。先按 [安装前提](Documentation/GettingStarted.md) 准备这些包，或在项目集成中拆分相应程序集引用。HybridCLR 热更新功能仍按需启用，此处是当前包结构的编译限制。

在 Package Manager 的 Samples 中导入“基础示例”，或查看 [Samples~/Basics](Samples~/Basics/README.md)。示例覆盖资源请求持有与释放、无需预制体的窗口、事件订阅、Controller 生命周期和 Editor 构建。

完整步骤见 [快速开始](Documentation/GettingStarted.md) 和 [示例说明](Documentation/Examples.md)。

## 核心功能

- ETask / ETask<T>：基于结构体、池化 awaiter 和状态机的异步任务，减少分配；实际 GC 取决于调用方式。
- 资源管理：按资源名加载、依赖管理、IResRequest 生命周期持有，Editor 模式可不打包调试。
- UI 窗口：类型名对应预制体，支持同步/异步打开、窗口组件、带参打开和可选 Roslyn UI 绑定。
- Controller、泛型事件、FSM、对象池、输入、音频、场景、Sprite 与 Shader 管理。
- DLC 基础版本、文件下载和资源发布；DLC 独立分包下载暂不可用。
- 可选 HybridCLR 集成，需要安装插件并配置、验证项目热更新流程。
- Profiler：独立程序集的运行时性能与错误面板、Shader 变体错误通知。
- Editor 工具链：资源导入、资源生成、AssetBundle、DLC 与 Player 构建及扩展。

ECS 类型仍保留，但当前没有公开 WorldManager / F.WorldManager，FWorld 构造函数为 internal。当前 Controller 帧更新和批量退出也有实现限制，见 [Review](REVIEW.md)。

## 初始化与业务入口

```csharp
using EasyFramework;
using UnityEngine;

public sealed class Bootstrap : MonoBehaviour
{
    private void Start() => StartAsync().Forget();

    private async ETask StartAsync()
    {
        await F.InitializeAsync();
        await GameController.Instance.EnterAsync();
    }
}

public sealed class GameController : Controller<GameController>
{
    protected override void OnEnter() => Debug.Log("Game entered");
}
```

资源、窗口与 DLC 使用前等待初始化。业务通过 F.xxx 访问模块，通过自己的 Controller<T>.Instance 进入和退出 Controller。当前没有 F.Initialize() 或 F.ControllerManager.EnterAsync<T>()。

## 资源配置

打开 `Project Settings > EasyFramework Tools > AssetBundleBuilder`，设置 Build Directories，默认为 Assets/Res_DLC：

- 普通文件按文件名分别打包，Cube.prefab 对应 Cube.ab。
- 以 .ab 结尾的目录整体打包。
- 跳过 Editor 目录和 .meta，并应用文件名、扩展名过滤。
- 多个 IAssetBundleBuilderSettings 会合并配置并去重。

业务只传资源名；长期使用资源时传入 IResRequest，并在结束使用时销毁实例、以同一个请求释放资源。完整异步清理示例见 [ResLoader](Documentation/Runtime/ResLoader.md)。

## 应用设置与发布

1. 通过 `Create > EasyFramework > AppSettings` 创建应用配置，设置公司、产品、包标识、版本与 URL。
2. 在 `Project Settings > EasyFramework` 的 App Settings 字段引用它。
3. 检查当前 Build Target 与 Build Settings 中启用的启动场景。
4. 使用 DLC_StreamingAssets 时，在 DLCBuilder 设置中开启 copyVersionToStreamingAssets；releaseVersion 留空使用最新资源版本。
5. 执行 `EasyFramework/Tools/PlayerBuilder - Execute`。

PlayerBuilder 的 assetBuilder 和 preSettingsEnabled 默认均为 true，会先构建资源，再同步应用设置并构建 Player：

```csharp
using EasyFramework.Editor;

public static class GameBuild
{
    public static void Execute() => PlayerBuilder.Instance.Execute();
}
```

需要分别运行资源和 Player 构建时，先关闭 PlayerBuilder.assetBuilder，再按 AssetBuilder -> PlayerBuilder 顺序执行。DLC 手动复制菜单为 `EasyFramework/Tools/DLCBuilder - CopyVersionToStreamingAssets`。

## Runtime

业务程序集引用 EasyFramework，入口从 [F 总览](Documentation/Runtime/F.md) 开始：

| 文档 | 职责 |
| --- | --- |
| [ETask](Documentation/Runtime/ETask.md) | 异步任务与生命周期 |
| [Settings](Documentation/Runtime/EasyFrameworkSettings.md) / [Behaviour](Documentation/Runtime/FBehaviour.md) | 配置、宿主与帧循环 |
| [ControllerManager](Documentation/Runtime/ControllerManager.md) | 业务流程 |
| [Event](Documentation/Runtime/Event.md) / [异步事件](Documentation/Runtime/AsyncEvent.md) | 同步派发与异步逐个等待事件 |
| [ResLoader](Documentation/Runtime/ResLoader.md) / [DLCManager](Documentation/Runtime/DLCManager.md) | 资源与版本 |
| [WindowManager](Documentation/Runtime/WindowManager.md) | 窗口与 UI |
| [SceneLoader](Documentation/Runtime/SceneLoader.md) / [SpriteLoader](Documentation/Runtime/SpriteLoader.md) / [ShaderLoader](Documentation/Runtime/ShaderLoader.md) | 场景、Sprite、Shader |
| [HttpManager](Documentation/Runtime/HttpManager.md) | HTTP 与下载 |
| [InputManager](Documentation/Runtime/InputManager.md) / [PoolManager](Documentation/Runtime/PoolManager.md) / [AudioPlayer](Documentation/Runtime/AudioPlayer.md) | 输入、对象池、音频 |
| [HybridCLRManager](Documentation/Runtime/HybridCLRManager.md) | 可选热更新 |
| [ECS（当前边界）](Documentation/Runtime/WorldManager.md) | 保留类型与缺少的入口 |
| [Profiler](Documentation/Runtime/Profiler.md) | 独立 EasyFramework.Profiler 程序集 |

## Editor.Tools

Editor 工具代码只能放在 Editor 目录或程序集，引用 EasyFramework.Editor。

| 文档 | 职责 |
| --- | --- |
| [ToolBase](Documentation/Editor.Tools/ToolBase.md) | 工具生命周期与扩展 |
| [AssetImporter](Documentation/Editor.Tools/AssetImporter.md) / [AssetCreator](Documentation/Editor.Tools/AssetCreator.md) | 导入与生成 |
| [AssetBuilder](Documentation/Editor.Tools/AssetBuilder.md) | 资源构建入口 |
| [AssetBundleBuilder](Documentation/Editor.Tools/AssetBundleBuilder.md) | Bundle 与 Manifest |
| [HybridCLRBuilder](Documentation/Editor.Tools/HybridCLRBuilder.md) | 可选程序集输出 |
| [DLCBuilder](Documentation/Editor.Tools/DLCBuilder.md) | 版本化资源与 StreamingAssets 复制 |
| [PlayerBuilder](Documentation/Editor.Tools/PlayerBuilder.md) | 应用设置与 Player 发布 |

## 目录结构

- Runtime/：核心运行时。
- Editor/：编辑器工具。
- Module/Profiler/：独立性能监测模块。
- Documentation/：入门、示例、Runtime 与 Editor 文档。
- Samples~/Basics/：可通过 UPM 导入的基础示例。
- Tests/Editor/：ETask 等现有编辑器测试。
- Plugins/、Res/、Resources/：生成器与框架资源。

文档基于当前源码核对；审查发现与验证范围见 [REVIEW.md](REVIEW.md)。许可证见 [LICENSE](LICENSE)。
