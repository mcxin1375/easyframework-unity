# F.Settings

`F.Settings` 的类型是 `EasyFrameworkSettings`，来源于 `Runtime/Base/ProjectSettings/EasyFrameworkSettings.cs`。它集中保存框架初始化、资源、DLC、HTTP、UI、HybridCLR 和调试参数。

## 常用配置

| 配置 | 作用 |
| --- | --- |
| `autoInitialize` | 是否由 Unity 回调自动发起初始化，默认 false；业务仍应 await 初始化 |
| `appSettings` | 应用级设置来源，包含 CDN 等项目配置 |
| `abSuffix` | AssetBundle 文件后缀，默认 `.ab` |
| `resLoaderEditorMode` | Editor 中是否使用编辑器资源桥接，默认 true |
| `resLoaderMode` | DLC 使用 StreamingAssets 或 CDN |
| `resRequestAliveTime` | 无活跃持有者时的资源缓存时间，默认 60 秒；长期使用应传 IResRequest |
| `streamingAssetsResRelativePath` | StreamingAssets 下的 DLC 相对目录，默认 DLC |
| `dlcVersionIndex` | 底包记录的 DLC 版本索引 |
| `downloadParallel` | DLC 下载并行数，默认 `3` |
| `unzipParallel` | 解压并行数，默认 `3` |
| `maxRetryCount` / `retryDelayMs` | 下载重试参数 |
| `uiRoot` / `resolution` / `uiRenderMode` | UI 根节点、分辨率和渲染模式 |
| `enterType` / `enterMethod` | HybridCLR 入口类型和方法名 |
| `debugLevel` | 调试日志级别 |

配置创建时计算 StreamingAssetsDLCPath、DataPath、DLCPath、ConfigPath、DownloadPath 和 DownloadTempPath；运行时创建所需目录，Editor 非 Play 模式不创建这些持久目录。iOS 使用 temporaryCachePath，其他当前分支使用 persistentDataPath。业务读取属性，不硬编码路径。PlayerBuilder 和 DLCBuilder 是独立 Editor 设置，发布默认行为见对应工具文档。

## 编辑器入口

在 Unity 中打开 `EasyFramework/Settings...` 修改项目设置。`F.Settings` 是运行时读取入口；设置资源的生成和保存由框架的 Project Settings 系统处理。

源码：[EasyFrameworkSettings.cs](../../Runtime/Base/ProjectSettings/EasyFrameworkSettings.cs)。
