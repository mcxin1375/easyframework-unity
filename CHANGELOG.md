# 更新日志

## [1.0.2] - 2026-11-24

### 新增

- AssetBundle 模拟分析基于临时序列化数据检测跨包内容重复，分别统计同对象重复和内容重复，并估算重复数据的字节数。模拟结果新增“写入资源”列表，显示 SBP 实际写入的资源。
- AssetBundle 窗口增加代码、Shader 和“直接资源数大于 1”筛选。悬浮重复资源可查看写入该资源的 Bundle。
- AssetBundle 布局报告升级至第 3 版，记录资源加载索引。窗口标黄同包内完全相同的索引。
- AssetBundleBuilder 支持扫描 `Packages/...` 下的资源，兼容带 `@版本` 的 Package 路径。

### 变更

- `WindowConfigAttribute` 统一配置窗口层级、同层前置窗口和关闭后的回收时间。默认层级为 `HUD`，回收时间为 60 秒，设为 0 可关闭自动回收。可通过 `IWindow.UILayer` 查询窗口层级。
- AssetBundle 名统一转为小写，同名构建定义合并。资源地址采用去掉扩展名的原始文件名并保留大小写，同包保留不同路径下的同名资源。
- AssetBundle 构建列表按直接资源标记重复配置。若构建布局仍有包名冲突（忽略大小写），构建会终止并输出冲突包名及直接资源路径。窗口将冲突包名标红并置顶。
- AssetBundle 模拟分析只在当前活动场景有未保存修改时提示保存，Shader 和 ComputeShader 导入对象的脏状态不再阻止分析。
- `CreateObj` 和 `CreateObjAsync` 在包名与资源地址相同时走单参数加载。Editor 资源桥接的单参数加载优先匹配地址，可回退到包内首个匹配类型的资源。
- SVCCollector 将新收集的 Shader 变体增量合并到已有集合。
- UPM 包版本更新至 `1.0.2`。

### 修复

- 修复扫描 `Assets/...` 和 `Packages/...` 子目录时生成的资源路径缺少配置目录前缀的问题。
- Editor 资源桥接卸载场景时使用解析后的场景路径。
- 将 `UnityDebugger` 的 `HideInCallstack` 标记限定在编辑器编译中。

### 升级提示

- 从 `IWindowManager.Open` 和 `OpenAsync` 的调用中移除 `UILayer` 参数，在窗口类型上用 `[WindowConfig(Layer = UILayer.Popup)]` 指定层级。自行实现 `IWindow` 的类型需提供 `UILayer`。
- 将 `WindowOpenBeforeAttribute` 迁移到 `WindowConfigAttribute.OpenBeforeType`，将 `Window.KeepAliveTime` 的配置迁移到 `WindowConfigAttribute.DestroyTime`。
- `ITimerObject.IsTimerAlive` 更名为 `Alive`，`UGUIHelper` 更名为 `UIHelper`。
- 按新规则重打包后，将原来使用完整资源路径加载同名资源的调用改为使用资源地址。地址完全相同的资源可通过 `LoadAllAssets<T>` 批量获取。

## [1.0.1] - 2026-11-20

### 新增

- 新增 AssetBundle 构建布局报告：正式构建保存 `AssetBundleBuildLayout.json`，`AssetBundleWindow` 支持查看正式布局、模拟分析当前配置，并生成独立的分析报告。
- `IResLoader` 新增按 `abName` 与 `assetName` 精确加载合包子资源的同步与异步接口；`IResLoaderEx` 的 `CreateObj` 系列同步支持该加载方式。
- 新增 AssetBundle 构建布局数据模型与 Editor 测试覆盖。

### 变更

- 重构 AssetBundle 构建工具与窗口，区分直接资源和间接依赖，记录资源引用来源，并按对象标识检测跨 Bundle 重复对象。
- 更新 AssetBundleBuilder、ResLoader、快速开始和 README 文档，补充模拟分析与合包资源加载说明。
- UPM 包版本更新至 `1.0.1`。

### 修复

- 修复 `ProfilerGUIBehaviour` 在 Unity 原生构造阶段创建 `ProfilerRecorderEx` 的初始化时机问题，改为在 `Awake` 中初始化并支持安全销毁。

## [1.0.0] - 2026-11-07

### 新增

- 按 Runtime 与 Editor 模块提供可从 Unity Package Manager 导入的示例。
- 补充快速开始、模块接口、资源生命周期和 Editor 构建工具文档。

### 变更

- UPM 包版本更新至 `1.0.0`。
- 将运行时代码与 Editor 工具分别整理到 `EasyFramework/Runtime` 和 `EasyFramework/Editor`，Profiler 独立放置于 `EasyFramework.Profiler`。
- 将文档目录由 `Documentation/Editor.Tools` 调整为 `Documentation/Editor`，同步更新 README 与源码链接。
- 清理旧版 Socket API；使用旧 `FSocket`、`TChannel` 或 `ReceiveBuffer` 类型的项目需要迁移到新的 socket channel 与 ring buffer API。

### 升级提示

- 源码目录已调整。若项目脚本直接定位包内文件，请改用 `EasyFramework/Runtime`、`EasyFramework/Editor` 和 `EasyFramework.Profiler` 下的新路径。

### 修复

- 修复 Profiler 计数器归零后界面仍显示旧值的问题。
