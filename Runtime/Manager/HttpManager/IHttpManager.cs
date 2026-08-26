/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.Threading;

namespace EasyFramework
{
    public readonly struct HttpDownloadRequest
    {
        public readonly string URL;
        public readonly string File;
        public HttpDownloadRequest(string url, string file)
        {
            URL = url;
            File = file;
        }
    }

    public interface IHttpManager
    {
        void GetString(string url, Action<bool, string> completeAction);
        ETask<string> GetStringAsync(string url, CancellationToken cancellationToken = default);
        
        void DownloadFile(string url, string file, Action<bool> completeAction);
        ETask<bool> DownloadFileAsync(string url, string file, CancellationToken cancellationToken = default);
        
        void DownloadFiles(HttpDownloadRequest[] requests, Action<bool> completeAction);
        ETask<bool> DownloadFilesAsync(HttpDownloadRequest[] requests, CancellationToken cancellationToken = default);
    }
}