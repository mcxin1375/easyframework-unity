# HybridCLRBuilder

`HybridCLRBuilder` 在启用 `EF_HYBRIDCLR` 编译符号并安装 HybridCLR 后可用。它负责生成 HybridCLR 热更新程序集和裁剪后元数据文件，并实现 `IToolEvent<AssetBuilder>`，因此会在 `AssetBuilder` 执行时自动触发。

## 菜单

| 菜单 | 作用 |
| --- | --- |
| `EasyFramework/Tools/HybridCLRBuilder - Execute` | 生成 HybridCLR 输出 |
| `... - Execute (HybridCLR-CompileDllActiveBuildTarget)` | 先编译当前平台 DLL，再生成输出 |
| `... - Execute (HybridCLR-GenerateAll)` | 先执行 HybridCLR `GenerateAll`，再生成输出 |

直接使用菜单执行时会走 `ToolBase.Execute()`。由 `AssetBuilder` 触发时，执行 `OnSelfExecute`，调用 HybridCLR 生成并复制输出。

## 配置

配置位于 `Project Settings > EasyFramework Tools > HybridCLRBuilder`：

| 字段 | 说明 |
| --- | --- |
| `loadAll` | 运行时是否将所有热更新程序集列为加载程序集，默认开启 |
| `customLoadDlls` | `loadAll` 关闭时自定义需要加载的程序集 |
| `stripDlls` | 需要补充元数据的裁剪程序集 |
| `hybridClrGenerateAll` | PlayerBuilder 前置阶段是否执行 HybridCLR `GenerateAll`，默认开启 |

热更新程序集来自 HybridCLR 的 `hotUpdateAssemblies` 和 `hotUpdateAssemblyDefinitions` 配置。

## 输出

工具会清空并重建当前平台的 `HybridCLRBuilder` 工具输出目录：

- 热更新 DLL 输出为 `<程序集名>.bytes`。
- PDB 输出为 `<程序集名>.pdb.bytes`。
- 裁剪程序集输出为 `<程序集名>.bytes`，用于补充元数据。
- 写入 `HybridCLRBuilderVersion.json`，记录程序集列表、加载列表和工具版本。

输出目录会被 `DLCBuilder` 作为 DLC 来源之一收集。构建前应确保 HybridCLR 已完成安装、当前平台配置有效，并确认程序集文件确实生成。

源码：[HybridCLRBuilder.cs](../../Editor/Tools/AssetBuilder/HybridCLRBuilder/HybridCLRBuilder.cs)、[HybridCLRBuilderSettings.cs](../../Editor/Tools/AssetBuilder/HybridCLRBuilder/HybridCLRBuilderSettings.cs)、[HybridCLRBuilderUtility.cs](../../Editor/Tools/AssetBuilder/HybridCLRBuilder/HybridCLRBuilderUtility.cs)。
