# EasyFramework 使用规范

本文件用于指导 AI 在使用 EasyFramework 功能时遵循框架约定。规范范围仅限 EasyFramework 的 API 使用、功能接入和相关代码组织。

## 总体约定

- 业务代码通过全局入口 `F.xxx` 访问运行时模块，不直接依赖模块的具体实现类。
- 使用资源、窗口、DLC 或其他依赖框架初始化的功能前，必须先调用一次并等待 `await F.InitializeAsync()` 完成。
- Runtime 代码不能引用 Editor 程序集；Editor 工具、构建逻辑和编辑器扩展应放在 Editor 程序集或 Editor 目录中。
- 新增功能时优先使用已有接口和扩展点，并阅读对应的 Runtime 或 Editor.Tools 文档后再实现。

## 文档读取要求

根据当前任务选择需要阅读的文档，不要求每次任务都读取 `Documentation` 目录下的全部文档：

- 涉及整体架构或框架初始化时，阅读 `Documentation/Runtime/F.md`。
- 涉及某个 `F.xxx` 模块时，阅读对应的 Runtime 文档。
- 涉及 Editor 工具时，先阅读 `Documentation/Editor.Tools/ToolBase.md`。
- 涉及具体 Editor 工具时，再阅读 `Documentation/Editor.Tools/` 下对应的工具文档。
- 涉及多个模块或完整发布流程时，补充阅读相关依赖模块的文档。
- 只有在文档审查、重构或整体整理时，才读取 `Documentation` 目录下的全部文档。

## 新增 Window 界面

新增 Window 时按以下流程处理：

1. 定义继承 `Window` 的窗口类型，并根据需要重写 `OnOpen`、`OnClose` 等生命周期方法。
2. 准备窗口资源，并确保资源名与窗口类型名 `Type.Name` 一致。默认 `Window` 会通过 `F.ResLoader` 按类型名加载并实例化资源。
3. 在初始化完成后通过 `F.WindowManager.OpenAsync<T>(layer)` 打开窗口，业务流程应等待返回的 `ETask<T>`。
4. 暂时隐藏窗口使用 `F.WindowManager.Close<T>()`；不再使用并需要释放实例时使用 `F.WindowManager.Destroy<T>()`。
5. 需要参数时，让窗口实现对应的 `ITParams<T1...>` 接口，并使用匹配泛型参数顺序的 `Open` 或 `OpenAsync` 重载。
6. 资源名与窗口类型名不一致时，使用 `WindowResources` 或 `WindowResourcesPath` 明确指定资源路径，不要在业务调用处重复实现窗口资源加载逻辑。

详细接口和窗口定义方式见 [WindowManager](Documentation/Runtime/WindowManager.md)。

## AssetBundle 资源

资源打包配置位于 `Project Settings > EasyFramework Tools > AssetBundleBuilder`，在 `Build Directories` 中配置基础目录，默认目录为 `Assets/Res_DLC`。

AssetBundleBuilder 遍历所有配置的基础目录：

- 以 `.ab` 结尾的目录，其目录内全部资源打包为一个 Bundle。
- 其他资源按文件分别打包，每个资源单独生成一个 Bundle。
- 遍历跳过 `Editor` 目录和 `.meta` 文件，并应用文件名和扩展名过滤配置。
- 所有 `IAssetBundleBuilderSettings` 配置会自动发现、合并和去重。

资源加载时只传入资源名，不在业务代码中拼接 Bundle 文件名、哈希文件名或下载路径：

```csharp
GameObject cube = await F.ResLoader.CreateObjAsync("Cube");
```

使用完资源后释放对应资源引用：

```csharp
F.ResLoader.Unload("Cube");
```

资源加载和释放接口以 [ResLoader](Documentation/Runtime/ResLoader.md) 为准，打包规则以 [AssetBundleBuilder](Documentation/Editor.Tools/AssetBundleBuilder.md) 为准。

## 发布配置

发布配置使用 `AppSettings`：

1. 在 Project 窗口右键选择 `Create > EasyFramework > AppSettings` 创建配置对象。
2. 配置应用名称、公司名称、产品名称、Bundle Identifier、版本号、Build Index、DLC URL 和 App Version URL。
3. 在 `Project Settings > EasyFramework` 的 `App Settings` 字段中引用该对象。

Runtime 和 Editor 工具通过 `EasyFrameworkSettings.AppSettings` 读取发布配置。

## 最终发布

资源和 Player 发布必须按顺序执行：

```csharp
using EasyFramework.Editor;

AssetBuilder.Instance.Execute();
PlayerBuilder.Instance.Execute();
```

`AssetBuilder` 会依次触发启用的 `HybridCLRBuilder`、`AssetBundleBuilder` 和 `DLCBuilder`，完成资源、版本信息和 DLC 输出；资源构建完成后再执行 `PlayerBuilder` 发布目标平台 Player。

也可以使用 Unity 菜单：

- `EasyFramework/Tools/AssetBuilder - Execute`
- `EasyFramework/Tools/PlayerBuilder - Execute`

详细工具接口和扩展方式见 [Editor.Tools](README.md#editortools) 及 `Documentation/Editor.Tools/` 下的对应文档。
