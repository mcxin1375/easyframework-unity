/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyFramework
{
    public class DownloadTask : ETask.TaskAwaiter<bool>, IHttpReceiver
    {
        private readonly List<Request> _requests = new();
        private readonly List<int> _pendingIndexes = new();
        private readonly List<int> _failedIndexes = new();
        private long[] _currentRequestBytes;
        private CancellationToken _cancellationToken;
        private int _activeCount;
        private bool _allLengthsValid = true;
        private int MaxParallel => EasyFrameworkSettings.Instance.downloadParallel > 0 ? EasyFrameworkSettings.Instance.downloadParallel : 1;
        private bool IsRunning => _activeCount > 0 || _pendingIndexes.Count > 0 || (Token != Guid.Empty && Status != ETaskStatusValue.Pending);
        private bool IsFinished => CompletedCount + _failedIndexes.Count == TotalCount;

        public int TotalCount => _requests.Count;
        public int CompletedCount { get; private set; }
        public long DownloadedBytes { get; private set; }
        public long CurrentBytes { get; private set; }
        public long TotalBytes { get; private set; }
        public float Progress
        {
            get
            {
                if (TotalCount == 0) return 0;
                var progress = _allLengthsValid
                    ? (TotalBytes > 0 ? (float)(DownloadedBytes + CurrentBytes) / TotalBytes : 0)
                    : (float)CompletedCount / TotalCount;
                return Mathf.Clamp01(progress);
            }
        }

        public void AddRequest(string url, string file, long length = 0)
        {
            if (_currentRequestBytes != null) throw new InvalidOperationException("Cannot add a request after download started.");

            var request = new Request(url, file, length);
            _requests.Add(request);
            if (length > 0) TotalBytes += length;
            else _allLengthsValid = false;
        }

        public ETask<bool> StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
            {
                FDebug.LogError("DownloadTask is already running.");
                return ETask.FromResult(false);
            }
            if (_currentRequestBytes == null)
            {
                _currentRequestBytes = new long[_requests.Count];
                if (_requests.Count == 0)
                {
                    return ETask.FromResult(true);
                }
            }
            if (IsFinished && _failedIndexes.Count == 0) return ETask.FromResult(true);

            _pendingIndexes.Clear();
            if (_failedIndexes.Count > 0)
            {
                _pendingIndexes.AddRange(_failedIndexes);
                _failedIndexes.Clear();
            }
            else if (!IsFinished)
            {
                for (var i = 0; i < _requests.Count; i++) _pendingIndexes.Add(i);
            }

            _cancellationToken = cancellationToken;
            Start(out var token);
            return new ETask<bool>(this, token);
        }

        protected override bool OnTaskTick()
        {
            if (_cancellationToken.IsCancellationRequested)
                while (_pendingIndexes.Count > 0)
                {
                    var index = _pendingIndexes[^1];
                    _pendingIndexes.RemoveAt(_pendingIndexes.Count - 1);
                    AddFailed(index);
                }

            while (!_cancellationToken.IsCancellationRequested && _activeCount < MaxParallel && _pendingIndexes.Count > 0)
            {
                var index = _pendingIndexes[^1];
                _pendingIndexes.RemoveAt(_pendingIndexes.Count - 1);
                _activeCount++;
                _ = DownloadAsync(index, _requests[index], _cancellationToken);
            }
            if (_activeCount == 0 && _pendingIndexes.Count == 0)
            {
                TrySetResult(_failedIndexes.Count == 0);
                return false;
            }
            return true;
        }

        protected override void OnTaskResult(ETaskStatus status)
        {
            _pendingIndexes.Clear();
            _cancellationToken = default;
            CurrentBytes = 0;
            if (_currentRequestBytes != null) Array.Clear(_currentRequestBytes, 0, _currentRequestBytes.Length);
        }

        private async ETask DownloadAsync(int index, Request request, CancellationToken cancellationToken)
        {
            var success = false;
            UnityWebRequest webRequest = null;
            try
            {
                webRequest = await ETask.UnityWebRequestDownload(request.URL, request.File, this, cancellationToken, index);
                success = webRequest != null && webRequest.result == UnityWebRequest.Result.Success;
            }
            catch (Exception e) { FDebug.LogException(e); }
            Complete(index, success, webRequest?.downloadedBytes ?? 0);
        }

        private void Complete(int index, bool success, ulong bytes)
        {
            CurrentBytes -= _currentRequestBytes[index];
            _currentRequestBytes[index] = 0;
            _activeCount--;
            if (success)
            {
                CompletedCount++;
                DownloadedBytes += bytes > long.MaxValue ? long.MaxValue : (long)bytes;
            }
            else AddFailed(index);
        }

        private void AddFailed(int index)
        {
            _failedIndexes.Add(index);
        }

        void IHttpReceiver.OnProgress(int requestIndex, ulong bytesReceived)
        {
            var bytes = bytesReceived > long.MaxValue ? long.MaxValue : (long)bytesReceived;
            CurrentBytes += bytes - _currentRequestBytes[requestIndex];
            _currentRequestBytes[requestIndex] = bytes;
        }

        public readonly struct Request
        {
            public readonly string URL;
            public readonly string File;
            public readonly long Length;

            public Request(string url, string file, long length)
            {
                URL = url;
                File = file;
                Length = length;
            }
        }
    }
}
