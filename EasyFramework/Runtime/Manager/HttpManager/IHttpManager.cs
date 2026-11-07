/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System.Threading;

namespace EasyFramework
{

    public interface IHttpManager
    {
        ETask<string> GetStringAsync(string url);
        ETask<bool> DownloadAsync(string url, string file, CancellationToken token = default);
        ETask<bool> DownloadAsync(string url, string file, int requestIndex, IHttpReceiver httpReceiver, CancellationToken token = default);
    }
}