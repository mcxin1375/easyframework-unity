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

## 缓存与版本切换

资源文件使用 DLCBuilder 生成的 MD5 文件名。`Exists` 同时校验文件存在、清单长度与内容 MD5；未知资源、残缺文件或内容损坏均返回 false。`GetFilePath` 只查询当前清单中的目标路径，不保证文件已经下载。

按需下载与批量更新共用文件请求，相同目标文件的并发请求只下载一次，并遵循 `downloadParallel` 上限。内容下载到同目录临时文件，校验通过后替换目标；失败不覆盖原文件，也不会把半包标记为可用。下载失败返回 false 或空路径，可再次调用重试。

新清单须通过 UID、资源名、文件名和长度校验，并成功保存后才替换内存中的版本；下载、解析或保存失败时保留旧清单及其资源服务器地址。更新失败仍返回错误，不会把旧清单的存在当作新版本更新成功。切换版本后路径直接来自当前清单，不保留上个版本的路径缓存。

缓存清理只处理 `DLCPath` 根层不属于当前版本的 MD5 资源文件，保留版本元数据、其他业务文件及子目录，不遍历应用数据根目录。`DLCPath` 应专用于框架缓存，业务数据放在其他目录。

源码：[IDLCManager.cs](../../Runtime/Manager/DLCManager/IDLCManager.cs)、[DLCManager.cs](../../Runtime/Manager/DLCManager/DLCManager.cs)。
