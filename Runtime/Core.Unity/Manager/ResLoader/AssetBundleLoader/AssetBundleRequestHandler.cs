using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework
{
    internal class AssetBundleRequestHandler : IPoolItem
    {
        
        public bool Alive
        {
            get
            {
                if (ReferList != null)
                {
                    foreach (var handler in ReferList) if (handler.Alive) return true;
                }
                return Time.time < AliveTime;
            }
        }
        public float AliveTime { get; private set; }
        public float AliveCountDownTime => Time.time > AliveTime ? 0 : AliveTime - Time.time;

        public string AbName;
        public AssetBundleInfo MainInfo;
        internal HashSet<IResRequest> ReferList;

        public void OnRent()
        {
            AliveTime = Time.time + AssetBundleLoader.Instance.ResRequestAliveTime;
        }
        public void OnReturn()
        {
            if (ReferList != null)
            {
                ReferList.Clear();
                ObjectPool<HashSet<IResRequest>>.Shared.Return(ReferList);
                ReferList = null;
            }

            MainInfo = null;
            AliveTime = 0;
            AbName = string.Empty;
        }
        public void OnDispose()
        {
            OnReturn();
        }

        public void InitInfo(string abName, AssetBundleInfo mainInfo)
        {
            AbName = abName;
            MainInfo = mainInfo;
            MainInfo.RefCount++;
            if (MainInfo.Dependencies?.Length > 0)
            {
                foreach (var depInfo in MainInfo.Dependencies) depInfo.RefCount++;
            }
        }

        private void AddRefer(IResRequest handler)
        {
            if (handler == null) return;
            if (ReferList == null) ReferList = ObjectPool<HashSet<IResRequest>>.Shared.Rent();
            ReferList.Add(handler);
        }

        private void RemoveRefer(IResRequest handler)
        {
            if (handler == null) return;
            ReferList?.Remove(handler);
        }

        public AssetBundle Load(IResRequest handler = null)
        {
            AliveTime = Time.time + AssetBundleLoader.Instance.ResRequestAliveTime;
            AddRefer(handler);

            if (MainInfo.Dependencies?.Length > 0)
            {
                foreach (var depInfo in MainInfo.Dependencies) depInfo.Load();
            }

            var ab = MainInfo.Load();
            return ab;
        }

        public ETask<AssetBundle> LoadAsync(IResRequest handler = null)
        {
            AliveTime = Time.time + AssetBundleLoader.Instance.ResRequestAliveTime;
            AddRefer(handler);

            if (MainInfo.Dependencies?.Length > 0)
            {
                foreach (var depInfo in MainInfo.Dependencies) depInfo.LoadAsync();
            }

            MainInfo.LoadAsync();

            return new ETask<AssetBundle>(Task.Create(MainInfo, Task.State.Loading, out var token), token);
        }

        public bool Unload(IResRequest handler, bool unloadAllLoadedObjects)
        {
            RemoveRefer(handler);
            if (ReferList != null)
            {
                foreach (var request in ReferList)
                    if (request.Alive) return false;
            }

            UnloadForce(unloadAllLoadedObjects);
            return true;
        }

        public void UnloadForce(bool unloadAllLoadedObjects)
        {
            if (MainInfo == null) return;

            MainInfo.RefCount--;
            if (MainInfo.RefCount <= 0) MainInfo.Unload(unloadAllLoadedObjects);

            if (MainInfo.Dependencies?.Length > 0)
            {
                foreach (var depInfo in MainInfo.Dependencies)
                {
                    depInfo.RefCount--;
                    if (depInfo.RefCount <= 0) depInfo.Unload(unloadAllLoadedObjects);
                }
            }

            MainInfo = null;
            ReferList?.Clear();
        }

        sealed class Task : ETask.TaskAwaiter<AssetBundle>
        {
            public enum State
            {
                Loading,
                Unloading,
            }

            private AssetBundleInfo _mainInfo;
            private State _state;

            protected override bool OnTaskTick()
            {
                if (IsCompleted())
                {
                    TrySetResult(_mainInfo.Bundle);
                    return false;
                }

                return true;
            }

            private bool IsCompleted()
            {
                switch (_state)
                {
                    case State.Loading:
                        if (_mainInfo.Dependencies?.Length > 0)
                        {
                            foreach (var depInfo in _mainInfo.Dependencies)
                            {
                                if (depInfo.State == AssetBundleInfo.AssetBundleState.Loading) return false;
                            }
                        }

                        return _mainInfo.State != AssetBundleInfo.AssetBundleState.Loading;
                    case State.Unloading:
                        if (_mainInfo.Dependencies?.Length > 0)
                        {
                            foreach (var depInfo in _mainInfo.Dependencies)
                            {
                                if (depInfo.State == AssetBundleInfo.AssetBundleState.Unloading) return false;
                            }
                        }

                        return _mainInfo.State != AssetBundleInfo.AssetBundleState.Unloading;
                }

                return false;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<Task>.Shared.Return(this);
            }

            public static Task Create(AssetBundleInfo mainInfo, State state, out Guid token)
            {
                var task = ObjectPool<Task>.Shared.Rent();
                task._mainInfo = mainInfo;
                task._state = state;
                task.Start(out token);
                return task;
            }
        }
    }
}