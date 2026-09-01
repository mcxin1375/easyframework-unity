# F.Settings

`F.Settings` 的类型是 `EasyFrameworkSettings`，来源于 `Runtime/Base/ProjectSettings/EasyFrameworkSettings.cs`。它集中保存框架初始化、资源、DLC、HTTP、UI、HybridCLR 和调试参数。

## 常用配置

| 配置 | 作用 |
| --- | --- |
| `autoInitialize` | 是否由 Unity 运行时初始化回调自动发起框架初始化 |
| `appSettings` | 应用级设置来源，包含 CDN 等项目配置 |
| `abSuffix` | AssetBundle 文件后缀，默认 `.ab` |
| `resLoaderEditorMode` | Editor 中是否使用编辑器资源加载桥接 |
| `resLoaderMode` | DLC 使用 StreamingAssets 或 CDN |
| `resRequestAliveTime` | 资源请求保持时间 |
| `dlcVersionIndex` | 底包记录的 DLC 版本索引 |
| `downloadParallel` | DLC 下载并行数，默认 `3` |
| `unzipParallel` | 解压并行数，默认 `3` |
| `maxRetryCount` / `retryDelayMs` | 下载重试参数 |
| `uiRoot` / `resolution` / `uiRenderMode` | UI 根节点、分辨率和渲染模式 |
| `enterType` / `enterMethod` | HybridCLR 入口类型和方法名 |
| `debugLevel` | 调试日志级别 |

初始化时会根据 Unity 平台设置 `DataPath`、`DLCPath`、`ConfigPath`、`DownloadPath` 和 `DownloadTempPath`，并创建所需目录。业务代码应读取这些属性，不要硬编码持久化路径。

## 编辑器入口

在 Unity 中打开 `EasyFramework/Settings...` 修改项目设置。`F.Settings` 是运行时读取入口；设置资源的生成和保存由框架的 Project Settings 系统处理。

源码：[EasyFrameworkSettings.cs](../../Runtime/Base/ProjectSettings/EasyFrameworkSettings.cs)。
