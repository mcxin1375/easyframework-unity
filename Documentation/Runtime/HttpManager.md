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

文件下载先写入目标目录内的唯一临时文件，响应与流关闭、已知长度校验通过后才替换最终文件。失败或取消返回 false，清理临时文件并保留原目标。通用 HTTP 下载校验服务端提供的长度；DLC 在此基础上按资源清单校验长度与 MD5。

源码：[IHttpManager.cs](../../EasyFramework/Runtime/Manager/HttpManager/IHttpManager.cs)。
