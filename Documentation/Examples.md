# 示例

在 `Package Manager > Easy Framework > Samples` 导入 **EasyFramework Samples**，或直接阅读包内 [Samples~](../Samples~)。Unity 默认将其导入 `Assets/Samples/<框架显示名>/<框架版本>/EasyFramework Samples`。

示例按模块平铺在 `Runtime` 和 `Editor` 两个目录，每个模块一个文件，分别包含 18 个运行时模块文件和 8 个 Editor 模块文件。一个文件可定义该模块需要的多个示例类，用法、资源名称和必要清理写在代码注释中，可像浏览 API 文档一样按需查看。示例没有独立说明 Markdown 或运行场景，导入不会自动执行业务；资源、URL 和发布配置由业务准备。

## Runtime

| 文件 | 用法 |
| --- | --- |
| [InitializationExample.cs](../Samples~/Runtime/InitializationExample.cs) | 等待框架初始化 |
| [EventExample.cs](../Samples~/Runtime/EventExample.cs) | `IEvent` / `IAsyncEvent` 对象与 `Action` / `Func` 委托的同步、异步订阅、派发和配对解绑 |
| [TaskExample.cs](../Samples~/Runtime/TaskExample.cs) | 完成与结果任务、全部 Delay、条件等待、消费与异常、Timer / Tick 注册移除、线程切换、对象与 Unity 请求适配器 |
| [LifecycleExample.cs](../Samples~/Runtime/LifecycleExample.cs) | 注册和移除框架帧循环 |
| [ControllerExample.cs](../Samples~/Runtime/ControllerExample.cs) | 生命周期、重复进入与完成事件、一至三个参数、组件与监听管理、管理器查询和自定义 `IController` |
| [WindowExample.cs](../Samples~/Runtime/WindowExample.cs) | 同名预制体、`IWindowUI` 自动绑定与组件、一至三个 `ITParams` 参数及 UI 组合、Resources 路径和 WindowManager 操作 |
| [ResourceExample.cs](../Samples~/Runtime/ResourceExample.cs) | Bundle、资产与实例的同步 / 异步加载、批量实例、状态和依赖查询、资源场景原语与请求配对释放 |
| [PoolExample.cs](../Samples~/Runtime/PoolExample.cs) | 对象池创建、预加载、租出、归还和指定 / 全局销毁 |
| [AudioExample.cs](../Samples~/Runtime/AudioExample.cs) | 音效、音乐和频道播放，句柄停止、播放查询、频道音量与静音 |
| [SceneExample.cs](../Samples~/Runtime/SceneExample.cs) | 场景同步 / 异步加载、卸载、进度与状态查询、激活和根对象查询 |
| [SpriteExample.cs](../Samples~/Runtime/SpriteExample.cs) | 按名或指定图集获取 Sprite、图集加载、引用清理与指定 / 全局释放 |
| [ShaderExample.cs](../Samples~/Runtime/ShaderExample.cs) | Shader 资源同步 / 异步加载和按名查询 |
| [InputExample.cs](../Samples~/Runtime/InputExample.cs) | 旧输入事件订阅与解绑 |
| [HttpExample.cs](../Samples~/Runtime/HttpExample.cs) | 文本 GET、文件下载、进度接收与调用方管理的取消令牌 |
| [DlcExample.cs](../Samples~/Runtime/DlcExample.cs) | 配置后的 DLC 更新与文件查询 |
| [HybridClrExample.cs](../Samples~/Runtime/HybridClrExample.cs) | 条件编译的 HybridCLR 入口 |
| [WorldExample.cs](../Samples~/Runtime/WorldExample.cs) | 已有 World 集成下的 System 定义 |
| [ProfilerExample.cs](../Samples~/Runtime/ProfilerExample.cs) | 显式启动与隐藏性能面板 |

## Editor

| 文件 | 用法 |
| --- | --- |
| [ToolBaseExample.cs](../Samples~/Editor/ToolBaseExample.cs) | 自定义工具、三阶段生命周期、扩展刷新与排序 |
| [AssetImporterExample.cs](../Samples~/Editor/AssetImporterExample.cs) | 资源导入配置创建与显式同步入口 |
| [AssetCreatorExample.cs](../Samples~/Editor/AssetCreatorExample.cs) | AssetCreator 配置扩展与显式执行入口 |
| [AssetBuilderExample.cs](../Samples~/Editor/AssetBuilderExample.cs) | 资源构建链的显式执行入口 |
| [AssetBundleBuilderExample.cs](../Samples~/Editor/AssetBundleBuilderExample.cs) | 资源目录与自定义构建配置、Bundle 构建、清单生成和重复检查 |
| [HybridCLRBuilderExample.cs](../Samples~/Editor/HybridCLRBuilderExample.cs) | 条件编译的热更程序集编译、GenerateAll 和构建入口 |
| [DLCBuilderExample.cs](../Samples~/Editor/DLCBuilderExample.cs) | DLC 构建与版本复制到 StreamingAssets |
| [PlayerBuilderExample.cs](../Samples~/Editor/PlayerBuilderExample.cs) | Player 构建参数、构建报告扩展与显式构建入口 |

框架模块的完整约定见 [F 总览](Runtime/F.md)，安装和发布前提见 [快速开始](GettingStarted.md)。类中的条件编译、配置占位或集成边界仍需按实际工程接入；查看代码或成功编译不能替代场景、构建和设备验证。
