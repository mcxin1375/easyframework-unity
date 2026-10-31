# DLCBuilder

DLCBuilder 将 AssetBundle 和可选 HybridCLR 输出整理成版本化 DLC，实现 IToolEvent<AssetBuilder>，在资源构建链末尾执行。

## 菜单与配置

配置：`Project Settings > EasyFramework Tools > DLCBuilder`。

| 菜单 | 作用 |
| --- | --- |
| `EasyFramework/Tools/DLCBuilder - Execute` | 构建 DLC |
| `EasyFramework/Tools/DLCBuilder - CopyVersionToStreamingAssets` | 复制选定版本到 StreamingAssets |

| 字段 | 说明 |
| --- | --- |
| `maxCacheNum` | 历史版本缓存数量，默认 10 |
| `buildName` | EDLCNameOptions.AppName 或 ToolVersion，默认后者 |
| `buildMode` | EDLCModeOptions，当前启用基础 DLC 模式 |
| `releaseVersion` | 要复制的版本，空值选最近一次构建 |
| `copyVersionToStreamingAssets` | 构建后自动复制，默认 false |

旧文档中的 buildNameType、buildOptions、EDLCMode 已不适用于当前配置。

## 构建与输出

构建会调用 DLC 工具扩展，收集 AssetBundleBuilder.ProjectPlatformPath 与启用时的 HybridCLRBuilder.ProjectPlatformPath；按版本名生成目录，写入 DLCVersionInfo.json、DLCVersion.json、LatestVersion.json 和 DLCBuilderVersion.json，再更新历史版本列表与缓存。

版本目录位于 DLCBuilder.ProjectPlatformPath 下。基础资源位于 DLC 子目录，每个文件以 MD5 重命名，索引保存原名、哈希名与长度。目录名默认来自工具版本 buildIndex；AppName 为空时回退到 buildIndex。

DLC 独立分包下载暂不可用，不能将基础版本构建等同于完整分包能力。

## 复制到 StreamingAssets

DLCBuilder.OnSelfExecuteAfter 在 copyVersionToStreamingAssets 为 true 时复制 releaseVersion 对应的版本。手动菜单也调用同一复制逻辑，并刷新 AssetDatabase。

复制目标为 EasyFrameworkSettings.StreamingAssetsDLCPath，默认 StreamingAssets/DLC；会按工具实现清理和重建目标。使用 DLC_StreamingAssets 的 Player 发布前需要完成此步骤。默认 PlayerBuilder 会先触发 AssetBuilder，可在 DLCBuilder 中开启自动复制。

## 扩展

在 Editor 程序集中实现 IToolEvent<DLCBuilder>，通过 Order 控制顺序。构建会清理并重建基础输出目录，不要将需要长期保留的文件放入该目录。

源码：[DLCBuilder.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilder.cs)、[DLCBuilderSettings.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderSettings.cs)、[DLCBuilderUtility.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs)。
