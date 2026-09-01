# F.HybridCLRManager

`F.HybridCLRManager` 只有在编译定义 `EF_HYBRIDCLR` 时才会出现在 `F` 中。它用于 HybridCLR 版本进入流程和程序集加载，依赖项目已安装并配置 HybridCLR。

| API | 说明 |
| --- | --- |
| `State` | 当前状态：`None`、`UpdateVersion`、`Downloading`、`Loading` 或 `Completed` |
| `Enter(Action<EResult>)` | 进入运行时 HybridCLR 流程 |
| `EnterEditor(Action<EResult>)` | 在 Editor 中进入加载流程 |
| `LoadAsync(string assemblyName, ELoadType)` | 加载 DLL 或元数据程序集 |

结果 `EResult` 包括 `Success`、`UpdateVersionError`、`DownloadError` 和 `LoadError`；`ELoadType` 为 `Dll` 或 `MetaData`。

当前实现的部分 Player 下载/加载路径仍依赖项目侧配置或注释中的后续接入逻辑。使用前应先验证 HybridCLR Builder 输出、DLC 文件名和加载入口，不要把此模块当作安装 package 后即可独立工作的热更方案。

源码：[HybridCLRManager.cs](../../Runtime/Manager/HybridCLRManager/HybridCLRManager.cs)。
