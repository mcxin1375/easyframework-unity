using System.Threading;

namespace EasyFramework.Samples.Modules
{
    // 进度回调报告当前累计字节与 requestIndex；由调用方在一次下载期间持有。
    public sealed class HttpProgressExample : IHttpReceiver
    {
        public int RequestIndex { get; private set; }
        public ulong BytesReceived { get; private set; }

        public void OnProgress(int requestIndex, ulong bytesReceived)
        {
            RequestIndex = requestIndex;
            BytesReceived = bytesReceived;
        }
    }

    // URL、可写文件路径和 CancellationToken 由调用方提供并管理，下载需检查 bool 结果。
    // 文本接口不支持取消；调用方处理异常，并在 await 后检查业务是否仍存活。
    public static class HttpExample
    {
        public static async ETask<string> GetTextAsync(string url)
        {
            await F.InitializeAsync();
            return await F.HttpManager.GetStringAsync(url);
        }

        public static async ETask<bool> DownloadAsync(string url, string filePath, CancellationToken cancellationToken)
        {
            await F.InitializeAsync();
            return await F.HttpManager.DownloadAsync(url, filePath, cancellationToken);
        }

        // 调用方持有取消源，需要取消时调用 Cancel()；这里仅接收令牌。
        public static async ETask<bool> DownloadWithProgressAsync(
            string url, string filePath, int requestIndex, IHttpReceiver receiver, CancellationToken cancellationToken)
        {
            await F.InitializeAsync();
            return await F.HttpManager.DownloadAsync(url, filePath, requestIndex, receiver, cancellationToken);
        }
    }
}
