/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System.Net.Http;
using System.Threading;
using UnityEngine.Networking;

namespace EasyFramework
{
    internal class HttpManager : Singleton<HttpManager>, IHttpManager
    {
        private const int BufferSize = 256 * 1024;
        private HttpClient HttpClient => Singleton<HttpClient>.Instance;

        public async ETask<string> GetStringAsync(string url)
        {
            var request = await ETask.UnityWebRequest(url);
            return request.downloadHandler.text;
        }
        
        public ETask<bool> DownloadAsync(string url, string file, CancellationToken token = default)
        {
            return DownloadAsync(url, file, -1, null, token);
        }
        public async ETask<bool> DownloadAsync(string url, string file, int requestIndex, IHttpReceiver httpReceiver, CancellationToken token = default)
        {
#if WEB_GL
            var webRequest = await ETask.UnityWebRequestDownload(url, file, httpReceiver, token);
            return webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success;
#else
            return await HttpClient.DownloadAsync(url, file, requestIndex, httpReceiver, token);
#endif
        }
    }
}
