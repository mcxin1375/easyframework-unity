# F.HttpManager

`F.HttpManager` 的类型是 `IHttpManager`，提供文本请求和文件下载。

| API | 说明 |
| --- | --- |
| `GetStringAsync(string url)` | 异步请求文本，返回 `ETask<string>` |
| `DownloadAsync(string url, string file, CancellationToken token)` | 下载到目标文件 |
| `DownloadAsync(string url, string file, int requestIndex, IHttpReceiver receiver, CancellationToken token)` | 下载并通过接收器报告请求进度或结果 |

```csharp
using EasyFramework;
using System.Threading;

sealed class DownloadExample
{
    public async ETask DownloadAsync(string url, string filePath, CancellationToken cancellationToken)
    {
        bool success = await F.HttpManager.DownloadAsync(url, filePath, cancellationToken);
    }
}
```

取消令牌由调用方持有并管理。应检查返回值，处理网络错误、取消和目标目录；不要假设 HTTP 状态或文件一定成功。

源码：[IHttpManager.cs](../../Runtime/Manager/HttpManager/IHttpManager.cs)。
