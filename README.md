# EasyFramework for Unity

EasyFramework 是一个面向 Unity 项目的轻量级 C# 开发框架，提供异步任务、事件、状态机、轻量 ECS、对象池、资源与场景加载、UI 窗口、网络和编辑器构建工具。

本目录是 EasyFramework 的核心开源子仓库。当前 package 版本为 `0.0.1`，API 仍可能变化，文档以当前源码为准。

## 快速开始

1. 包名为 `com.cookie.easyframework`，当前版本为 `0.0.1`。将本目录作为 Unity Package Manager 的本地包，或通过 Git URL 安装；Git 安装时使用包含 `package.json` 的仓库路径。
2. 确认项目包含以下依赖：Newtonsoft Json `3.2.2` 与 Scriptable Build Pipeline `1.21.25`。
3. 安装后确认 Console 没有包解析错误，并能引用 `EasyFramework`。编辑器菜单出现 `EasyFramework/Settings...` 与 `EasyFramework/Preferences...`，说明 Editor 程序集已加载。运行时代码只能引用运行时程序集，编辑器扩展应放在 Editor 程序集或 Editor 目录中。
4. 在游戏进入业务状态前调用一次并等待：

```csharp
using EasyFramework;

public sealed class Bootstrap
{
    public async ETask StartAsync() => await F.InitializeAsync();
}
```

`F.Initialize()` 只发起初始化而不等待；需要使用资源、窗口或 DLC 时必须等待 `F.InitializeAsync()`。详见 [F 总览](Documentation/Runtime/F.md)。

## Runtime

Runtime 程序集名称为 `EasyFramework`。业务代码以 `F.xxx` 访问运行时模块，不直接依赖模块的具体实现类。

### F 入口

| 入口 | 职责 |
| --- | --- |
| `F.WorldManager` / `F.ControllerManager` | World、Entity、System 与 Controller 生命周期 |
| `F.Event` | 泛型事件注册和派发 |
| `F.ResLoader` / `F.DLCManager` | AssetBundle 资源、版本和下载 |
| `F.WindowManager` | UI 窗口与层级 |
| `F.SceneLoader` / `F.SpriteLoader` / `F.ShaderLoader` | 场景、Sprite 和 Shader |
| `F.HttpManager` | HTTP 文本请求和文件下载 |
| `F.InputManager` / `F.PoolManager` / `F.AudioPlayer` | 输入、对象池和音频 |
| `F.Settings` / `F.Behaviour` | 配置与 Unity 生命周期宿主 |
| `F.HybridCLRManager` | 可选 HybridCLR 集成 |

### 文档

完成安装和初始化后，从 [F 总览](Documentation/Runtime/F.md) 开始，再按入口阅读：

- [F.Settings](Documentation/Runtime/EasyFrameworkSettings.md)
- [F.Behaviour](Documentation/Runtime/FBehaviour.md)
- [F.WorldManager](Documentation/Runtime/WorldManager.md)
- [F.ControllerManager](Documentation/Runtime/ControllerManager.md)
- [F.Event](Documentation/Runtime/Event.md)
- [F.ResLoader](Documentation/Runtime/ResLoader.md)
- [F.DLCManager](Documentation/Runtime/DLCManager.md)
- [F.HttpManager](Documentation/Runtime/HttpManager.md)
- [F.WindowManager](Documentation/Runtime/WindowManager.md)
- [F.SpriteLoader](Documentation/Runtime/SpriteLoader.md)
- [F.ShaderLoader](Documentation/Runtime/ShaderLoader.md)
- [F.SceneLoader](Documentation/Runtime/SceneLoader.md)
- [F.InputManager](Documentation/Runtime/InputManager.md)
- [F.PoolManager](Documentation/Runtime/PoolManager.md)
- [F.AudioPlayer](Documentation/Runtime/AudioPlayer.md)
- [F.HybridCLRManager](Documentation/Runtime/HybridCLRManager.md)

## Editor

Editor 程序集名称为 `EasyFramework.Editor`，只在 Unity Editor 中使用，不能从 Runtime 程序集引用。

### 工具入口

| 菜单 | 职责 |
| --- | --- |
| `EasyFramework/Settings...` | 框架项目设置 |
| `EasyFramework/Preferences...` | 用户偏好设置 |
| `EasyFramework/Tools/AssetCreator - Execute` | 执行资源创建工具 |
| `EasyFramework/Tools/AssetImporter - Execute` | 执行资源导入工具 |
| `EasyFramework/Tools/AssetBuilder - Execute` | 按顺序触发资源构建工具 |
| `EasyFramework/Tools/AssetBundleBuilder - Execute` | 执行 AssetBundle 构建 |
| `EasyFramework/Tools/AssetBundleBuilder - BuildManifestOnly` | 只生成 Manifest |
| `EasyFramework/Tools/AssetBundleBuilder - CheckVerifyValidity` | 检查资源是否重复进入多个 Bundle |
| `EasyFramework/Tools/DLCBuilder - Execute` | 执行 DLC 构建 |
| `EasyFramework/Tools/HybridCLRBuilder - Execute` | 执行 HybridCLR 构建 |
| `EasyFramework/Tools/PlayerBuilder - Execute` | 执行 Player 构建 |

### Editor.Tools

Editor 工具的通用执行模型、扩展接口和各工具的配置方式见以下文档：

| 文档 | 工具职责 |
| --- | --- |
| [ToolBase](Documentation/Editor.Tools/ToolBase.md) | 工具基类、执行生命周期和扩展发现 |
| [AssetImporter](Documentation/Editor.Tools/AssetImporter.md) | 资源目录导入与同步 |
| [AssetCreator](Documentation/Editor.Tools/AssetCreator.md) | 资源生成流程扩展入口 |
| [AssetBuilder](Documentation/Editor.Tools/AssetBuilder.md) | 统一触发资源构建工具 |
| [HybridCLRBuilder](Documentation/Editor.Tools/HybridCLRBuilder.md) | HybridCLR 热更新程序集构建 |
| [AssetBundleBuilder](Documentation/Editor.Tools/AssetBundleBuilder.md) | AssetBundle 构建与 Manifest 检查 |
| [DLCBuilder](Documentation/Editor.Tools/DLCBuilder.md) | 版本化 DLC 资源打包 |
| [PlayerBuilder](Documentation/Editor.Tools/PlayerBuilder.md) | Unity Player 发布构建 |

### Editor 扩展约定

Editor 工具通过 `IToolExtension` 扩展配置，通过 `IToolEvent<T>` 插入工具执行前、执行中和执行后的处理。新增 Editor 工具时，应提供配置类型、设置入口、菜单入口，并在本 README 的 Editor 部分补充索引；详细说明统一放在 `Documentation/Editor.Tools/`。

## 目录结构

- `Runtime/`：运行时程序集 `EasyFramework`
- `Editor/`：仅编辑器程序集 `EasyFramework.Editor`
- `Documentation/Runtime/`：运行时入口和模块文档
- `Documentation/Editor.Tools/`：Editor 工具及扩展接口文档
- `Plugins/`：第三方插件与协议相关资源
- `Res/`、`Resources/`：框架使用的资源

## 许可证

详见 [LICENSE](LICENSE)。
