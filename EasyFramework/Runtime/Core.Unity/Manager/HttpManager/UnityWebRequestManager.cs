/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyFramework
{
    internal class UnityWebRequestManager : Singleton<UnityWebRequestManager>, IHttpManager
    {
        public void GetString(string url, Action<bool, string> completeAction)
        {
            _ = GetStringAsync(url, completeAction);
        }
        private async ETask GetStringAsync(string url, Action<bool, string> completeAction)
        {
            var request = await GetUnityWebRequestAsync(url);
            completeAction?.Invoke(request.result == UnityWebRequest.Result.Success, request.downloadHandler.text);
        }

        public async ETask<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = await GetUnityWebRequestAsync(url, cancellationToken);
            return request.downloadHandler.text;
        }

        public void DownloadFile(string url, string file, Action<bool> completeAction)
        {
            _ = DownloadFileAsync(url, file, completeAction);
        }
        public void DownloadFiles(HttpDownloadRequest[] requests, Action<bool> completeAction)
        {
            _ = DownloadFileAsync(requests, completeAction);
        }
        private async ETask DownloadFileAsync(string url, string file, Action<bool> completeAction)
        {
            var result = await DownloadFileAsync(url, file);
            completeAction?.Invoke(result);
        }
        private async ETask DownloadFileAsync(HttpDownloadRequest[] requests, Action<bool> completeAction)
        {
            var result = await DownloadFilesAsync(requests);
            completeAction?.Invoke(result);
        }

        public async ETask<bool> DownloadFileAsync(string url, string file, CancellationToken cancellationToken = default)
        {
            var request = await DownloadUnityWebRequestAsync(url, file, cancellationToken);
            return request.result == UnityWebRequest.Result.Success;
        }

        public async ETask<bool> DownloadFilesAsync(HttpDownloadRequest[] requests, CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < requests.Length; i++)
            {
                var request = await DownloadUnityWebRequestAsync(requests[i].URL, requests[i].File, cancellationToken);
                if (request.result != UnityWebRequest.Result.Success)
                {
                    return false;
                }
            }
            return true;
        }


        public ETask<UnityWebRequest> GetUnityWebRequestAsync(string url, CancellationToken cancellationToken = default)
        {
            return new ETask<UnityWebRequest>(UnityWebRequestTask.Create(url, cancellationToken, out var token), token);
        }
        public ETask<UnityWebRequest> DownloadUnityWebRequestAsync(string url, string file, CancellationToken cancellationToken = default)
        {
            return new ETask<UnityWebRequest>(UnityWebRequestDownloadTask.Create(url, file, cancellationToken, out var token), token);
        }
        
        private sealed class UnityWebRequestTask : ETask.TaskAwaiter<UnityWebRequest>
        {
            private string _url;
            private int _errorRetry;
            private float _lockTime = 0;

            private CancellationToken _cancellationToken;
            private UnityWebRequest _unityWebRequest;

            private int MaxRetryCount => EasyFrameworkAOTSettings.Instance.maxRetryCount;
            protected override bool OnTaskTick()
            {
                if (Time.time < _lockTime) return true;

                if (_unityWebRequest == null)
                {
                    _unityWebRequest = UnityWebRequest.Get(_url);
                    _unityWebRequest.SendWebRequest();
                }

                if (_unityWebRequest.isDone)
                {
                    bool isSuccess = _unityWebRequest.result == UnityWebRequest.Result.Success;
                    
                    FDebug.Log($"StartRequest：{_url}, {_unityWebRequest.result}");
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

                        _lockTime = Time.time + EasyFrameworkAOTSettings.Instance.retryDelayMs / 1000.0f;
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
                _unityWebRequest = UnityWebRequest.Get(_url);
                _unityWebRequest.SendWebRequest();
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                _url = string.Empty;
                _unityWebRequest?.Dispose();
                _unityWebRequest = null;
                
                ObjectPool<UnityWebRequestTask>.Shared.Return(this);
            }
            
            public static UnityWebRequestTask Create(string url, CancellationToken cancellationToken, out Guid token)
            {
                var task = ObjectPool<UnityWebRequestTask>.Shared.Rent();
                task._url = url;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }
        
        private sealed class UnityWebRequestDownloadTask : ETask.TaskAwaiter<UnityWebRequest>
        {
            private string _url;
            private string _file;
            private string _tempFile;
            private int _errorRetry;
            private CancellationToken _cancellationToken;
            private UnityWebRequest _unityWebRequest;

            private enum EState
            {
                None,
                Downloading,
                Success,
                Failure
            }
            private EState _state = EState.None;

            private int MaxRetryCount => EasyFrameworkAOTSettings.Instance.maxRetryCount;

            protected override bool OnTaskTick()
            {
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
                            bool isSuccess = _unityWebRequest.result == UnityWebRequest.Result.Success;
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
                
                _unityWebRequest = UnityWebRequest.Get(_url);
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
                _unityWebRequest?.Dispose();
                _unityWebRequest = null;
                
                ObjectPool<UnityWebRequestDownloadTask>.Shared.Return(this);
            }
            
            public static UnityWebRequestDownloadTask Create(string url, string file, CancellationToken cancellationToken, out Guid token)
            {
                var task = ObjectPool<UnityWebRequestDownloadTask>.Shared.Rent();
                task._url = url;
                task._file = file;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }
    }
}