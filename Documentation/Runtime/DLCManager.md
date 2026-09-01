# F.DLCManager

`F.DLCManager` 的类型是 `IDLCManager`，负责 DLC 版本更新、文件路径查询、存在性检查和下载。DLC 依赖 `F.Settings` 中的版本、路径、CDN 和重试配置。

| API | 说明 |
| --- | --- |
| `UpdateAsync()` | 按当前配置执行 DLC 更新 |
| `UpdateAsync(string dlcVersion)` | 更新到指定 DLC 版本 |
| `GetFileName(string resName)` | 查询资源对应的文件名 |
| `GetFilePath(string resName)` | 查询资源本地文件路径 |
| `Exists(string resName)` | 判断本地资源是否存在 |
| `DownloadAsync(string resName)` | 下载指定资源并返回成功状态 |
| `DownloadAsync(string resName, out string filePath)` | 下载并输出文件路径 |
| `DownloadAndReturnFileAsync(string resName)` | 下载并返回文件路径 |

`UpdateAsync` 返回 `IDLCManager.EResult`：`Success`、`InitVersionError` 或 `DLCUpdaterError`。初始化阶段会先等待 DLC 管理器完成，再初始化 AssetBundle loader；业务资源加载应位于 `await F.InitializeAsync()` 之后。

源码：[IDLCManager.cs](../../Runtime/Manager/DLCManager/IDLCManager.cs)、[DLCManager.cs](../../Runtime/Manager/DLCManager/DLCManager.cs)。
