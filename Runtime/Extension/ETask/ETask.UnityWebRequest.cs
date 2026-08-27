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
        
        public static ETask<UnityWebRequest> UnityWebRequestDownload(string url, string file, IHttpReceiver httpReceiver = null, CancellationToken cancellationToken = default, int requestIndex = -1)
        {
            return new ETask<UnityWebRequest>(UnityWebRequestDownloadTask.Create(url, file, out var token, httpReceiver, cancellationToken, requestIndex), token);
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
            private int _errorRetry;
            private CancellationToken _cancellationToken;
            private UnityWebRequest _unityWebRequest;
            private IHttpReceiver _httpReceiver;
            private int _requestIndex;

            private enum EState
            {
                None,
                Downloading,
                Success,
                Failure
            }
            private EState _state = EState.None;

            private int MaxRetryCount => EasyFrameworkSettings.Instance.maxRetryCount;

            protected override bool OnTaskTick()
            {
                var bytesReceived = _unityWebRequest?.downloadedBytes ?? 0;
                _httpReceiver?.OnProgress(_requestIndex, bytesReceived);
                switch (_state)
                {
                    case EState.Failure:
                        TrySetResult(_unityWebRequest);
                        return false;
                    case EState.Success:
                        TrySetResult(_unityWebRequest);
                        return false;
                    case EState.Downloading:

                        if (_cancellationToken.IsCancellationRequested)
                        {
                            _state = EState.Failure;
                            return true;
                        }

                        if (_unityWebRequest == null)
                        {
                            StartRetry();
                            return true;
                        }

                        if (_unityWebRequest.isDone)
                        {
                            bool isSuccess = _unityWebRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success;
                            if (isSuccess)
                            {
                                FileHelper.DeleteFile(_file);
                                File.Move(_tempFile, _file);
                                _state = EState.Success;
                            }
                            else
                            {
                                StartRetry();
                            }
                        }

                        break;
                }
                return true;
            }

            protected override void OnTaskStart()
            {
                _errorRetry = 0;
                _state = EState.None;
                _tempFile = _file + ".download";
                
                string dir = Path.GetDirectoryName(_file);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                StartRequest();
            }

            private void StartRetry()
            {
                _errorRetry++;

                if (_errorRetry >= MaxRetryCount)
                {
                    _state = EState.Failure;
                    return;
                }
                StartRequest();
            }

            private void StartRequest()
            {
                _state = EState.Downloading;

                if (_unityWebRequest != null)
                {
                    _unityWebRequest.Dispose();
                    _unityWebRequest = null;
                }

                FileHelper.DeleteFile(_tempFile);

                FDebug.Log($"StartRequest：{_url}, {_tempFile}");
                
                _unityWebRequest = UnityEngine.Networking.UnityWebRequest.Get(_url);
                var downloadHandler = new DownloadHandlerFile(_tempFile);
                downloadHandler.removeFileOnAbort = true;
                _unityWebRequest.downloadHandler = downloadHandler;
                _unityWebRequest.SendWebRequest();
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                FileHelper.DeleteFile(_tempFile);
                _url = string.Empty;
                _file = string.Empty;
                _tempFile = string.Empty;
                _httpReceiver = null;
                _requestIndex = -1;
                _unityWebRequest?.Dispose();
                _unityWebRequest = null;
                
                ObjectPool<UnityWebRequestDownloadTask>.Shared.Return(this);
            }
            
            public static UnityWebRequestDownloadTask Create(string url, string file, out Guid token, IHttpReceiver httpReceiver = null, CancellationToken cancellationToken = default, int requestIndex = -1)
            {
                var task = ObjectPool<UnityWebRequestDownloadTask>.Shared.Rent();
                task._url = url;
                task._file = file;
                task._httpReceiver = httpReceiver;
                task._requestIndex = requestIndex;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }
    }
}
