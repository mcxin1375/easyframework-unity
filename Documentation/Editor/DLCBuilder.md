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

ToolBase 统一调用 DLC 工具扩展。Utility 收集 AssetBundleBuilder.ProjectPlatformPath 与 HybridCLR 已启用时的 HybridCLRBuilder.ProjectPlatformPath；先在暂存目录生成哈希资源、DLCVersionInfo.json、DLCVersion.json 和 DLCBuilderVersion.json，校验后再提交版本目录、LatestVersion.json 和历史列表。缺失源目录、不可读文件、复制或校验失败都抛异常，停止父构建链。

版本目录位于 DLCBuilder.ProjectPlatformPath 下。基础资源位于 DLC 子目录，每个文件以 MD5 重命名，索引保存原名、哈希名与长度。目录名默认来自工具版本 buildIndex；AppName 为空时回退到 buildIndex。

DLC 独立分包下载暂不可用，不能将基础版本构建等同于完整分包能力。

资源复制成功且长度、MD5 与源一致后才进入索引。相同资源名对应不同内容时拒绝构建；相同内容可以共用哈希文件。历史版本裁剪也参与提交和回滚；自动复制明确选定的历史 releaseVersion 会额外保留该版本，避免构建完成后选择指向已被裁剪的目录。

## 复制到 StreamingAssets

`copyVersionToStreamingAssets` 为 true 时，Utility 在本次 DLC 发布中一起准备 releaseVersion 对应的底包；空值选择本次构建。手动菜单也使用相同校验与暂存复制逻辑，并刷新 AssetDatabase。

复制目标为 EasyFrameworkSettings.StreamingAssetsDLCPath，默认 StreamingAssets/DLC。先验证版本元数据、清单 UID、每个文件的长度和 MD5，将所需文件平铺到暂存目录并复核，再替换目标。无效版本或复制失败会保留原底包，构建明确失败。DLC Utility 将本次版本目录、LatestVersion、历史列表、历史裁剪和自动复制的底包作为同一组文件显式替换；替换中失败仅恢复本组产物。Utility 已经成功返回后，后续工具或 After 回调失败不撤销已完成发布。使用 DLC_StreamingAssets 的 Player 发布前需要完成此步骤。默认 PlayerBuilder 会先触发 AssetBuilder，可在 DLCBuilder 中开启自动复制。

## 扩展

在 Editor 程序集中实现 IToolEvent<DLCBuilder>，通过 Order 控制顺序。直接或通过 AssetBuilder 执行时，Before → Execute → After 各调用一次；直接调用 Utility 不分发扩展事件。构建会替换基础输出目录，不要将需要长期保留的文件放入该目录。

源码：[DLCBuilder.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilder.cs)、[DLCBuilderSettings.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderSettings.cs)、[DLCBuilderUtility.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs)。
