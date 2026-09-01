# AssetBundleBuilder

`AssetBundleBuilder` 负责根据配置生成 AssetBundle 和依赖 Manifest，同时实现 `IToolEvent<AssetBuilder>`，会在 `AssetBuilder` 的资源构建链中自动执行。

## 菜单

| 菜单 | 作用 |
| --- | --- |
| `EasyFramework/Tools/AssetBundleBuilder - Execute` | 按当前配置构建 AssetBundle |
| `EasyFramework/Tools/AssetBundleBuilder - BuildManifestOnly` | Dry Run 构建并生成可供编辑器查询的 Manifest 资产 |
| `EasyFramework/Tools/AssetBundleBuilder - CheckVerifyValidity` | 检查同一资源是否被多个 Bundle 重复收集 |

## 配置入口

基础配置位于 `Project Settings > EasyFramework Tools > AssetBundleBuilder`，对应 `AssetBundleBuilderSettings`：

| 字段 | 说明 |
| --- | --- |
| `createManifestFile` | 正式构建前是否先生成 Manifest 资产 |
| `buildAssetBundleOptions` | 传给 Scriptable Build Pipeline 的构建选项 |
| `buildDirectories` | 自动扫描的目录集合 |
| `ignoreFileNames` | 按文件名排除资源，不含扩展名 |
| `ignoreFileExes` | 按扩展名排除资源 |

默认扫描目录是 `Assets/Res_DLC`。实际项目应按资源布局修改，不要假设默认目录一定存在。

## 两种资源配置方式

### 创建 AssetBundleBuildSettings

在 Project 窗口右键选择：

`Create > EasyFramework > AssetBuilder > AssetBundleBuildSettings`

对象实现 `IAssetBundleBuilderSettings`，字段如下：

| 字段 | 说明 |
| --- | --- |
| `enabled` | 是否参与构建 |
| `buildDirectories` | 需要自动扫描的目录 |
| `buildInfos` | 显式定义的 Bundle 配置 |

`AssetBundleBuildInfo` 包含 `abName`、`abResType` 和 `directories`。`abResType` 当前支持 `All` 和 `Shader`；Shader 类型会使用 `AssetDatabase.FindAssets("t:Shader")` 收集 Shader。

### 实现 IAssetBundleBuilderSettings

在 Editor 程序集中创建实现类，可以按代码或其他项目配置动态返回目录和 Bundle：

```csharp
using EasyFramework.Editor;

internal sealed class GameAssetBundleBuildSettings : IAssetBundleBuilderSettings
{
    public int Order => 10;

    public string[] BuildDirectories => new[]
    {
        "Assets/Game/Resources"
    };

    public AssetBundleBuildInfo[] BuildInfos => new[]
    {
        new AssetBundleBuildInfo
        {
            abName = "game-shaders",
            abResType = EAssetBundleBuildResType.Shader,
            directories = new[] { "Assets/Game/Shaders" }
        }
    };
}
```

`BuildDirectories` 和 `BuildInfos` 可以返回 `null`。所有配置对象和代码扩展都会被自动发现，按 `Order` 升序收集；同名 Bundle 的资源列表会合并并去重。

## 目录扫描规则

- 跳过 `Editor` 目录和 `.meta` 文件。
- `ignoreFileNames` 和 `ignoreFileExes` 在自动扫描时生效。
- 普通文件按不含扩展名的文件名生成 Bundle 名。
- 以 `.ab` 结尾的目录会整体生成一个 Bundle，Bundle 名去掉 `.ab` 后缀。
- 显式 `BuildInfos` 先生成，再与目录扫描结果按 Bundle 名合并。
- Bundle 名缺少当前 `EasyFrameworkSettings.abSuffix` 时会自动追加后缀。

路径应使用 Unity 资源路径或当前工程可访问的路径，并避免同一资源被多个配置重复添加。

## 输出与检查

正式构建输出到 `AssetBundleBuilder.ProjectPlatformPath`，并写入 `AssetBundleManifest.json` 及依赖关系。构建过程中会删除输出目录中不再属于当前构建列表的旧 Bundle。

`BuildManifestOnly` 使用 Dry Run，在 `AssetBundleBuilder.AssetsPlatformPath` 下生成 `<平台名>.asset`，便于编辑器查询构建结果。正式构建前建议先执行 `CheckVerifyValidity`，处理所有重复资源错误后再发布。

## 构建链路

直接执行工具会升级 `AssetBundleBuilder` 自身版本。通过 `AssetBuilder` 触发时，工具作为 `IToolEvent<AssetBuilder>` 执行 `BuildBySettings`；它的输出会被后续 `DLCBuilder` 收集。

源码：[AssetBundleBuilder.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilder.cs)、[AssetBundleBuilderSettings.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderSettings.cs)、[AssetBundleBuildSettings.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuildSettings.cs)、[AssetBundleBuilderUtility.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderUtility.cs)。
