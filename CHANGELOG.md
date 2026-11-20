# 更新日志

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
