# AssetBundleBuilder

`AssetBundleBuilder` 负责根据配置生成 AssetBundle 和依赖 Manifest，同时实现 `IToolEvent<AssetBuilder>`，会在 `AssetBuilder` 的资源构建链中自动执行。

## 菜单

| 菜单 | 作用 |
| --- | --- |
| `EasyFramework/Tools/AssetBundleBuilder - Execute` | 按当前配置构建 AssetBundle |
| `EasyFramework/Tools/AssetBundleBuilder - BuildManifestOnly` | Dry Run 构建并生成可供编辑器查询的 Manifest 资产 |
| `EasyFramework/Tools/AssetBundleBuilder - CheckVerifyValidity` | 检查同一资源是否被多个 Bundle 重复收集 |

`AssetBundleWindow` 提供构建列表、最近正式构建和模拟分析三个页签。构建列表显示直接资源和间接资源；模拟分析额外显示“写入资源”，其中间接资源统一来自 `AssetDatabase.GetDependencies()`，写入资源来自 SBP 实际写入对象，因此同一路径可以同时出现在间接资源和写入资源列表。引用数列统计包含该资源的 AB 数量，悬浮资源路径可查看所在 AB 列表；SBP 生成的 `temp:/`、`archive:/` 和 `library:/` 内部对象不计入资源列表或统计。顶部开关为“重复资源”“代码”和“Shader”，AB 列表顶部另有“直接资源数>1”筛选，代码与 Shader 默认不计入列表及统计，打开对应开关后纳入显示。重复统计分为“同对象重复”和“内容重复”：前者要求相同 `ObjectIdentifier` 实际写入多个 AB，后者要求不同对象的临时序列化数据哈希相同，并给出未压缩序列化数据的重复字节估算。构建列表只显示直接资源的重复配置；间接资源仍展示但不作为重复证据，直接重复资源和对应 AB 会标色提醒。最近正式构建显示对象级结果，内容级结果需要报告包含内容分析；模拟分析会生成内容分析。AB 名称只在直接资源数大于 1 时显示 `(n)`，不显示间接资源数量。模拟分析开始前会检测未保存的项目资源，并在用户确认后保存再运行；Shader 和 ComputeShader 导入对象的 Unity dirty 状态不作为未保存资源拦截。分析调用 `AssetBundleBuilderUtility.AnalyzeBySettings()`，模拟分析会写入隔离的临时序列化文件，在序列化写入后捕获对象数据，然后在归档 AssetBundle 前停止；不会写正式 AssetBundle 文件。分析会编译 Player 脚本，并把脚本输出、临时输出和构建日志放入隔离目录后清理；SBP/Unity 构建缓存仍可能更新。

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
- `.ab` 目录中的文件默认使用去掉扩展名的文件名作为 Bundle 内地址名；同一 Bundle 内不同路径出现同名文件时，冲突资源使用完整 Unity 资源路径作为地址名。
- 显式 `BuildInfos` 先生成，再与目录扫描结果按 Bundle 名合并。
- Bundle 名缺少当前 `EasyFrameworkSettings.abSuffix` 时会自动追加后缀。

路径应使用 Unity 资源路径或当前工程可访问的路径，并避免同一资源被多个配置重复添加。

## 输出与检查

正式构建先写入输出目录旁的暂存目录，验证非空构建结果、各 Bundle 文件和 `AssetBundleManifest.json` 后，再替换 `AssetBundleBuilder.ProjectPlatformPath`。新输出只包含当前构建产物；构建失败抛异常，保留旧目录并阻止后续 DLC / Player 构建。

正式构建成功后，布局报告写入 `AssetBundleBuilder.DebugPlatformPath/AssetBundleBuildLayout.json`；模拟结果单独写入 `AssetBundleBuildLayout.Analysis.json`，不会覆盖正式构建报告。报告按资源路径列出各包实际写入的对象、直接/间接来源及引入依赖的直接资源。只有同一路径下相同 `ObjectIdentifier` 对象进入多个 AB，才标记为重复；同一源文件的不同子对象不会仅因路径相同而误判。报告不包含压缩后的精确体积，体积仍以正式 AB 文件为准。

`BuildManifestOnly` 保留原职责：在隔离目录运行管线，再于 `AssetBundleBuilder.AssetsPlatformPath` 下更新 `<平台名>.asset`，保留已有 GUID。正式发布配置含 `DryRunBuild` 时直接失败，避免父链发布旧资源。正式构建前建议先执行 `CheckVerifyValidity`，处理所有重复登记资源后再发布。

## 构建链路

直接执行或由 AssetBuilder 触发时，都会通过完整 Execute() 升级 AssetBundleBuilder 版本，再在 OnSelfExecute 中调用 BuildBySettings；输出由后续 DLCBuilder 收集。

源码：[AssetBundleBuilder.cs](../../EasyFramework/Editor/Tools/AssetBundleBuilder/AssetBundleBuilder.cs)、[AssetBundleBuilderSettings.cs](../../EasyFramework/Editor/Tools/AssetBundleBuilder/AssetBundleBuilderSettings.cs)、[AssetBundleBuildSettings.cs](../../EasyFramework/Editor/Tools/AssetBundleBuilder/AssetBundleBuildSettings.cs)、[AssetBundleBuildLayout.cs](../../EasyFramework/Editor/Tools/AssetBundleBuilder/AssetBundleBuildLayout.cs)、[AssetBundleBuilderUtility.cs](../../EasyFramework/Editor/Tools/AssetBundleBuilder/AssetBundleBuilderUtility.cs)、[AssetBundleWindow.cs](../../EasyFramework/Editor/Window/AssetBundleWindow/AssetBundleWindow.cs)。
