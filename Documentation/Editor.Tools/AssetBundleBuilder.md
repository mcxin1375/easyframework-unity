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

每次收集构建配置都会重新发现配置资产，默认目录通过实时 getter 读取当前设置。同一 Editor 会话内修改目录，以及新增、禁用或删除 `AssetBundleBuildSettings`，下一次构建立即生效。

## 目录扫描规则

- 跳过 `Editor` 目录和 `.meta` 文件。
- `ignoreFileNames` 和 `ignoreFileExes` 在自动扫描时生效。
- 普通文件按不含扩展名的文件名生成 Bundle 名。
- 以 `.ab` 结尾的目录会整体生成一个 Bundle，Bundle 名去掉 `.ab` 后缀。
- 显式 `BuildInfos` 先生成，再与目录扫描结果按 Bundle 名合并。
- Bundle 名缺少当前 `EasyFrameworkSettings.abSuffix` 时会自动追加后缀。

路径应使用 Unity 资源路径或当前工程可访问的路径，并避免同一资源被多个配置重复添加。

## 输出与检查

正式构建先写入输出目录旁的暂存目录，验证非空构建结果、各 Bundle 文件和 `AssetBundleManifest.json` 后，再替换 `AssetBundleBuilder.ProjectPlatformPath`。新输出只包含当前构建产物；构建失败抛异常，保留旧目录并阻止后续 DLC / Player 构建。

`BuildManifestOnly` 带 Dry Run 标志，在隔离目录运行管线，再于 `AssetBundleBuilder.AssetsPlatformPath` 下更新 `<平台名>.asset`，保留已有 GUID。当前 SBP 兼容入口仍可能生成 Bundle，这些临时产物会被删除，不覆盖正式输出。正式发布配置含 `DryRunBuild` 时直接失败，避免父链发布旧资源。正式构建前建议先执行 `CheckVerifyValidity`，处理所有重复资源错误后再发布。

## 构建链路

直接执行或由 AssetBuilder 触发时，都会通过完整 Execute() 升级 AssetBundleBuilder 版本，再在 OnSelfExecute 中调用 BuildBySettings；输出由后续 DLCBuilder 收集。

源码：[AssetBundleBuilder.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilder.cs)、[AssetBundleBuilderSettings.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderSettings.cs)、[AssetBundleBuildSettings.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuildSettings.cs)、[AssetBundleBuilderUtility.cs](../../Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderUtility.cs)。
