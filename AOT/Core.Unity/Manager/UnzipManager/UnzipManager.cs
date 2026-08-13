/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace EasyFramework
{
    public struct UnzipRequest
    {
        public readonly string TargetFile;
        public readonly string DestinationDir;
        public UnzipRequest(string targetFile, string destinationDir)
        {
            TargetFile = targetFile;
            DestinationDir = destinationDir;
        }
    }
    public class UnzipManager : Singleton<UnzipManager>
    {
        public bool IsBusying { get; private set; }
        public float Progress => UnzippedCount / (float)_totalCount + _inProgressDict.Values.Sum() * _inProgressPercent;

        private readonly Queue<UnzipRequest> _requestQueue = new();
        private readonly ConcurrentQueue<UnzipRequest> _errorQueue = new();
        private readonly ConcurrentDictionary<string, float> _inProgressDict = new();
        private readonly Action<string, float> _progressAction;
        private readonly int _parallel;
        private int UnzippedCount => _totalCount - _requestQueue.Count - _inProgressDict.Count - _errorQueue.Count;
        private int _totalCount;
        private float _inProgressPercent;

        public UnzipManager()
        {
            _parallel = Math.Min(EasyFrameworkAOTSettings.Instance.unzipParallel, Environment.ProcessorCount);
            _progressAction = InProgressAction;
        }
        
        private void InProgressAction(string targetFile, float progress)
        {
            _inProgressDict[targetFile] = progress;
        }

        public void Reset()
        {
            _requestQueue.Clear();
            _errorQueue.Clear();
            _inProgressDict.Clear();
            _totalCount = 0;
        }

        public void AddRequest(string zipFile, string destinationDir)
        {
            if (IsBusying)
            {
                Debug.LogError("DLCUnzipEx is busying");
                return;
            }
            
            _requestQueue.Enqueue(new UnzipRequest(zipFile, destinationDir));
            _totalCount++;
            _inProgressPercent = 1f / _totalCount;
        }

        public async ETask<bool> DoAsync(CancellationToken token = default)
        {
            if (IsBusying)
            {
                Debug.LogError("DLCUnzipEx is busying");
                return false;
            }

            IsBusying = true;

            _inProgressDict.Clear();
            while (_errorQueue.Count > 0)
            {
                if (_errorQueue.TryDequeue(out var item))
                    _requestQueue.Enqueue(item);
            }
            while (_requestQueue.Count > 0)
            {
                if (token.IsCancellationRequested) return false;
                if (_errorQueue.Count > 0) return false;
                
                if (_inProgressDict.Count < _parallel && _requestQueue.Count > 0)
                {
                    var request = _requestQueue.Dequeue();
                    _ = DoRequestAsync(request, token);
                }

                await ETask.DelayFrame();
            }
            await ETask.WaitUntil(() => _inProgressDict.Count == 0);

            IsBusying = false;
            return _errorQueue.Count == 0;
        }

        private async ETask DoRequestAsync(UnzipRequest unzipRequest, CancellationToken token = default)
        {
            await ETask.RunOnThreadPool(() =>
            {
                _inProgressDict.TryAdd(unzipRequest.TargetFile, 0);
                bool result = ZipHelper.UnzipFile(unzipRequest.TargetFile, unzipRequest.DestinationDir, _progressAction, token);
                if (!result)
                {
                    _errorQueue.Enqueue(unzipRequest);
                }
            }, cancellationToken: token);
            
            _inProgressDict.TryRemove(unzipRequest.TargetFile, out var _);
        }
        
    }
}