# DLCBuilder

`DLCBuilder` 负责把资源构建结果整理成带版本信息的 DLC。它实现 `IToolEvent<AssetBuilder>`，默认在 AssetBundle 和 HybridCLR 输出完成后被 `AssetBuilder` 触发。

## 菜单

`EasyFramework/Tools/DLCBuilder - Execute`

## 构建流程

执行 `BuildBySettings` 时会：

1. 执行 DLC 扩展的 `OnExecuteBefore`。
2. 根据配置生成版本名和版本目录。
3. 收集 `AssetBundleBuilder.ProjectPlatformPath`，以及启用 HybridCLR 时的 `HybridCLRBuilder.ProjectPlatformPath`。
4. 按 `EDLCMode` 生成 DLC 文件和 `DLCVersionInfo.json`。
5. 写入 `DLCVersion.json`、`LatestVersion.json` 和 `DLCBuilderVersion.json`。
6. 按 `maxCacheNum` 刷新历史版本列表。
7. 执行 DLC 扩展的 `OnExecuteAfter`。

当前 `EDLCMode` 只启用 `DLC` 基础模式；压缩包模式尚未启用。

## 配置

配置位于 `Project Settings > EasyFramework Tools > DLCBuilder`：

| 字段 | 说明 |
| --- | --- |
| `maxCacheNum` | 保留的历史版本数量，按时间排序 |
| `buildNameType` | 版本目录名来源：`AppName` 或 `ToolVersion` |
| `buildOptions` | DLC 构建模式，当前使用 `EDLCMode.DLC` |

使用 `AppName` 时，如果应用名为空会回退到工具版本的 `buildIndex`。默认版本目录名来自 `DLCBuilder.Version.buildIndex`。

## 输出结构

版本输出位于 `DLCBuilder.ProjectPlatformPath/<版本名>`，基础 DLC 位于其中的 `DLC` 子目录。每个源文件会按 MD5 重命名并记录原始资源名、哈希文件名和文件长度；版本信息通过 JSON 文件关联这些内容。

`DLCBuilderVersion.json` 同时记录 DLC、AssetBundle 和 HybridCLR 构建版本，便于追踪构建来源。历史版本由 `DLCBuilderVersionList.json` 管理，超出缓存数量的版本目录会被清理。

## 扩展

```csharp
using EasyFramework.Editor;

internal sealed class GameDlcExtension : IToolEvent<DLCBuilder>
{
    public int Order => 100;

    public void OnExecuteBefore()
    {
        // 校验资源版本或准备额外来源。
    }

    public void OnExecuteAfter()
    {
        // 上传、校验或生成项目自己的发布索引。
    }
}
```

DLC 扩展通过 `DLCBuilder.ToolEvents` 被发现。由于 DLC 构建会清空并重建基础 DLC 输出目录，扩展不应把需要长期保留的文件直接放在该目录中。

源码：[DLCBuilder.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilder.cs)、[DLCBuilderSettings.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderSettings.cs)、[DLCBuilderUtility.cs](../../Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs)、[DLCBuilderVersion.cs](../../Runtime/Base/Tools/DLCBuilderVersion.cs)。
