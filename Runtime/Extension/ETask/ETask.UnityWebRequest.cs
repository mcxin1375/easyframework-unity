/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using System.Threading;
using UnityEngine.Networking;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public static ETask<UnityWebRequest> UnityWebRequest(string url)
        {
            return new ETask<UnityWebRequest>(UnityWebRequestTask.Create(url, out var token), token);
        }
        
        public static ETask<UnityWebRequest> UnityWebRequestDownload(string url, string file, IHttpReceiver httpReceiver = null, CancellationToken cancellationToken = default, int requestIndex = -1, long length = -1, string md5 = null)
        {
            FDebug.Log($"{url} -> {file}");
            return new ETask<UnityWebRequest>(UnityWebRequestDownloadTask.Create(url, file, out var token, httpReceiver, cancellationToken, requestIndex, length, md5), token);
        }


        private sealed class UnityWebRequestTask : TaskAwaiter<UnityWebRequest>
        {
            private string _url;
            private int _errorRetry;
            private float _lockTime = 0;

            private CancellationToken _cancellationToken;
            private UnityWebRequest _unityWebRequest;

            private int MaxRetryCount => EasyFrameworkSettings.Instance.maxRetryCount;

            protected override bool OnTaskTick()
            {
                if (!IsPending) return base.OnTaskTick();
                if (UnityEngine.Time.time < _lockTime) return true;

                if (_unityWebRequest == null)
                {
                    _unityWebRequest = UnityEngine.Networking.UnityWebRequest.Get(_url);
                    _unityWebRequest.SendWebRequest();
                }

                if (_unityWebRequest.isDone)
                {
                    bool isSuccess = _unityWebRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success;
                    // FDebug.Log($"StartRequest：{_url}, {_unityWebRequest.result}");
                    if (isSuccess)
                    {
                        TrySetResult(_unityWebRequest);
                        return false;
                    }
                    else
                    {
                        _errorRetry++;
                        if (_errorRetry >= MaxRetryCount)
                        {
                            TrySetResult(_unityWebRequest);
                            return false;
                        }

                        _lockTime = UnityEngine.Time.time + EasyFrameworkSettings.Instance.retryDelayMs / 1000.0f;
                        _unityWebRequest.Dispose();
                        _unityWebRequest = null;
                    }
                }

                if (_cancellationToken.IsCancellationRequested)
                {
                    TrySetResult(null);
                    return false;
                }

                return true;
            }

            protected override void OnTaskStart()
            {
                _errorRetry = 0;
                _lockTime = 0;
                _unityWebRequest = UnityEngine.Networking.UnityWebRequest.Get(_url);
                _unityWebRequest.SendWebRequest();
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                _url = string.Empty;
                _unityWebRequest?.Dispose();
                _unityWebRequest = null;

                ObjectPool<UnityWebRequestTask>.Shared.Return(this);
            }

            public static UnityWebRequestTask Create(string url, out Guid token, CancellationToken cancellationToken = default)
            {
                var task = ObjectPool<UnityWebRequestTask>.Shared.Rent();
                task._url = url;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }

        private sealed class UnityWebRequestDownloadTask : TaskAwaiter<UnityWebRequest>
        {
            private string _url;
            private string _file;
            private string _tempFile;
            private long _length;
            private string _md5;
            private int _errorRetry;
            private CancellationToken _cancellationToken;
            private UnityWebRequest _unityWebRequest;
            private IHttpReceiver _httpReceiver;
            private int _requestIndex;

            private int MaxRetryCount => EasyFrameworkSettings.Instance.maxRetryCount;

            protected override bool OnTaskTick()
            {
                if (!IsPending) return base.OnTaskTick();
                if (_cancellationToken.IsCancellationRequested || _unityWebRequest == null)
                {
                    ClearRequest();
                    TrySetResult(null);
                    return false;
                }

                bool success = false;
                try
                {
                    _httpReceiver?.OnProgress(_requestIndex, _unityWebRequest.downloadedBytes);
                    if (!_unityWebRequest.isDone) return true;

                    if (_unityWebRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        // Release the file handle before validation and replacement.
                        _unityWebRequest.downloadHandler?.Dispose();
                        if (!FileHelper.VerifyFileIntegrity(_tempFile, _length, _md5))
                            throw new InvalidDataException($"Downloaded file validation failed: {_url}");
                        if (!_cancellationToken.IsCancellationRequested)
                        {
                            FileHelper.ReplaceFileFromTemp(_tempFile, _file);
                            success = true;
                        }
                    }
                }
                catch (Exception e) { FDebug.LogException(e); }

                if (success)
                {
                    TrySetResult(_unityWebRequest);
                    return false;
                }
                ClearRequest();
                _errorRetry++;
                if (_cancellationToken.IsCancellationRequested || _errorRetry >= MaxRetryCount)
                {
                    TrySetResult(null);
                    return false;
                }
                StartRequest();
                return true;
            }

            protected override void OnTaskStart()
            {
                _errorRetry = 0;
                _tempFile = null;
                _unityWebRequest = null;
                if (_cancellationToken.IsCancellationRequested) return;
                try
                {
                    _tempFile = FileHelper.CreateDownloadTempPath(_file);
                    StartRequest();
                }
                catch (Exception e) { FDebug.LogException(e); }
            }

            private void StartRequest()
            {
                try
                {
                    FDebug.Log($"StartRequest：{_url}, {_tempFile}");
                    _unityWebRequest = new UnityWebRequest(_url, "GET");
                    _unityWebRequest.downloadHandler = new DownloadHandlerFile(_tempFile) { removeFileOnAbort = true };
                    _unityWebRequest.SendWebRequest();
                }
                catch (Exception e)
                {
                    FDebug.LogException(e);
                    ClearRequest();
                }
            }

            private void ClearRequest()
            {
                if (_unityWebRequest != null)
                {
                    if (!_unityWebRequest.isDone) _unityWebRequest.Abort();
                    _unityWebRequest.Dispose();
                    _unityWebRequest = null;
                }
                FileHelper.DeleteFile(_tempFile);
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                ClearRequest();
                _url = string.Empty;
                _file = string.Empty;
                _tempFile = string.Empty;
                _length = -1;
                _md5 = null;
                _httpReceiver = null;
                _requestIndex = -1;
                _cancellationToken = default;
                ObjectPool<UnityWebRequestDownloadTask>.Shared.Return(this);
            }

            public static UnityWebRequestDownloadTask Create(string url, string file, out Guid token, IHttpReceiver httpReceiver = null, CancellationToken cancellationToken = default, int requestIndex = -1, long length = -1, string md5 = null)
            {
                var task = ObjectPool<UnityWebRequestDownloadTask>.Shared.Rent();
                task._url = url;
                task._file = file;
                task._length = length;
                task._md5 = md5;
                task._httpReceiver = httpReceiver;
                task._requestIndex = requestIndex;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }
    }
}
