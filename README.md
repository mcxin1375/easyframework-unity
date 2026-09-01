# EasyFramework for Unity

EasyFramework 是一个面向 Unity 项目的轻量级 C# 开发框架，提供异步任务、事件、状态机、轻量 ECS、对象池、资源与场景加载、UI 窗口、网络和编辑器构建工具。

## 快速开始

安装包后确认项目包含依赖 Newtonsoft Json `3.2.2` 和 Scriptable Build Pipeline `1.21.25`，Console 没有包解析错误，并能引用 `EasyFramework`。编辑器菜单出现 `EasyFramework/Settings...` 与 `EasyFramework/Preferences...`，说明 Editor 程序集已加载。

### 1. 资源打包及加载配置

打开 `Project Settings > EasyFramework Tools > AssetBundleBuilder`，在 `Build Directories` 中配置 AssetBundle 的基础目录。默认目录为：

`Assets/Res_DLC`

AssetBundleBuilder 会遍历所有配置的基础目录，并按以下规则生成 Bundle：

- 遇到以 `.ab` 结尾的目录时，将该目录下的全部资源打包成一个 Bundle；例如 `Cube.ab` 目录生成 `Cube.ab`。
- 其他资源按文件分别打包，每个资源单独生成一个 Bundle；例如 `Cube.prefab` 生成 `Cube.ab`。
- 遍历时跳过 `Editor` 目录和 `.meta` 文件，并应用 `ignoreFileNames`、`ignoreFileExes` 过滤配置。
- 所有配置对象提供的 `Build Directories` 会自动合并；同名 Bundle 的资源列表会合并并去重。

打包过程会生成 Bundle Manifest、依赖关系和 DLC 文件名映射，因此业务代码不需要维护 AssetBundle 文件名、哈希文件名或物理下载路径。资源加载只需要传入资源名，资源名应与打包资源的名称一致：

```csharp
using EasyFramework;
using UnityEngine;

public sealed class Bootstrap
{
    public async ETask StartAsync()
    {
        await F.InitializeAsync();
        GameObject cube = await F.ResLoader.CreateObjAsync("Cube");
    }
}
```

`F.InitializeAsync()` 只需在进入业务状态前调用一次并等待完成。使用完资源后，可通过 `F.ResLoader.Unload("Cube")` 释放对应资源；有请求对象时，应将同一个请求传给加载和释放接口。具体接口见 [F.ResLoader](Documentation/Runtime/ResLoader.md) 和 [AssetBundleBuilder](Documentation/Editor.Tools/AssetBundleBuilder.md)。

### 2. 发布配置

在 Project 窗口右键选择 `Create > EasyFramework > AppSettings` 创建 `AppSettings` 对象，配置应用发布信息：

| 配置 | 说明 |
| --- | --- |
| `App Name` | 应用名称，也可用于 DLC 版本目录名 |
| `Bundle Identifier` | 应用包标识 |
| `Ver 1`、`Ver 2`、`Ver 3` | 应用版本号，组合为 `Ver1.Ver2.Ver3` |
| `Build Index` | 应用构建索引 |
| `Dlc URL` | DLC 文件服务地址 |
| `App Version URL` | 应用版本信息地址，支持 `{Platform}` 占位符 |

然后打开 `Project Settings > EasyFramework`，将创建的对象设置到 `App Settings` 字段。运行时和 Editor 工具通过 `EasyFrameworkSettings.AppSettings` 读取这份配置。

### 3. 最终发布

在确认 AssetBundleBuilder、DLCBuilder 和 PlayerBuilder 配置完成后，按顺序执行资源版本构建和 Player 发布：

```csharp
using EasyFramework.Editor;

AssetBuilder.Instance.Execute();
PlayerBuilder.Instance.Execute();
```

`AssetBuilder.Execute()` 会按顺序触发 HybridCLRBuilder（启用时）、AssetBundleBuilder 和 DLCBuilder，生成资源、版本信息及 DLC 输出。资源构建完成后再执行 `PlayerBuilder.Execute()`，将应用设置、场景和 StreamingAssets 等内容发布为目标平台 Player。也可以使用 Unity 菜单 `EasyFramework/Tools/AssetBuilder - Execute` 和 `EasyFramework/Tools/PlayerBuilder - Execute` 执行相同流程。

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
