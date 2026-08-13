/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/8/7
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework
{
    public class SceneInfo : IResRequest
    {
        public readonly string SceneName;
        
        public bool Alive { get; private set; }
        public bool IsActive { get; private set; }
        
        public EResState State  { get; private set; } = EResState.Unloaded;
        public Scene Scene;
        public LoadSceneMode Mode;

        public float LoadingProgress
        {
            get
            {
                switch (State)
                {
                    case EResState.Loaded: return 1;
                    case EResState.Loading: return _loadAsyncOperation?.progress ?? 0;
                    default: return 0;
                }
            }
        }
        public float UnloadingProgress
        {
            get
            {
                switch (State)
                {
                    case EResState.Unloaded: return 1;
                    case EResState.Unloading: return _unloadAsyncOperation?.progress ?? 0;
                    default: return 0;
                }
            }
        }

        private AsyncOperation _loadAsyncOperation;
        private AsyncOperation _unloadAsyncOperation;

        internal SceneInfo(string sceneName)
        {
            SceneName = sceneName;
        }

        internal void Load(LoadSceneMode mode)
        {
            switch (State)
            {
                case EResState.Loaded: break;
                case EResState.Unloaded:
                    F.ResLoader.LoadScene(SceneName, mode, this);
                    break;
                case EResState.Loading:
                    FDebug.LogWarning($"Request Load({SceneName}), But state is [Loading] now");
                    break;
                case EResState.Unloading:
                    FDebug.LogWarning($"Request Load({SceneName}), But state is [Loading] now");
                    break;
            }
        }
        internal async ETask LoadAsync(LoadSceneMode mode)
        {
            switch (State)
            {
                case EResState.Loaded: break;
                case EResState.Unloaded:
                    _loadAsyncOperation = await F.ResLoader.LoadSceneAsync(SceneName, mode, this);
                    State = _loadAsyncOperation == null ? EResState.Unloaded : EResState.Loading;
                    await WaitLoadingAsync();
                    break;
                case EResState.Loading:
                    await WaitLoadingAsync();
                    break;
                case EResState.Unloading:
                    FDebug.LogWarning($"Request LoadAsync({SceneName}), But state is [Unloading] now");
                    await WaitUnloadingAsync();
                    _loadAsyncOperation = await F.ResLoader.LoadSceneAsync(SceneName, mode, this);
                    State = _loadAsyncOperation == null ? EResState.Unloaded : EResState.Loading;
                    await WaitLoadingAsync();
                    break;
            }
        }
        internal async ETask UnloadAsync()
        {
            switch (State)
            {
                case EResState.Unloaded: break;
                case EResState.Loaded:
                    _unloadAsyncOperation = await F.ResLoader.UnloadSceneAsync(SceneName, this);
                    State = _unloadAsyncOperation == null ? EResState.Unloaded : EResState.Unloading;
                    await WaitUnloadingAsync();
                    break;
                case EResState.Loading:
                    FDebug.LogWarning($"Request UnloadAsync({SceneName}), But state is [Loading] now");
                    await WaitLoadingAsync();
                    _unloadAsyncOperation = await F.ResLoader.UnloadSceneAsync(SceneName, this);
                    State = _unloadAsyncOperation == null ? EResState.Unloaded : EResState.Unloading;
                    await WaitUnloadingAsync();
                    break;
                case EResState.Unloading:
                    await WaitUnloadingAsync();
                    break;
            }
        }
        internal ETask WaitLoadingAsync()
        {
            return new ETask(Task.Create(this, EResState.Loading, out var token), token);
        }
        internal ETask WaitUnloadingAsync()
        {
            return new ETask(Task.Create(this, EResState.Unloading, out var token), token);
        }

        public void TrySetActive()
        {
            if (!Scene.IsValid()) return;
            SceneManager.SetActiveScene(Scene);
        }

        public GameObject FindSceneRootObj(string objName)
        {
            if (!Scene.IsValid()) return null;
            var arr = Scene.GetRootGameObjects();
            if (arr?.Length > 0) foreach (var o in arr) if (o.name == objName) return o;
            return null;
        }

        internal void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            Scene = scene;
            Mode = mode;
            State = EResState.Loaded;
            Alive = true;
            _loadAsyncOperation = null;

            // FDebug.Log($"OnSceneLoaded: {SceneName}, state: {State}", LogTag.EasyFramework);
        }
        internal void OnUnloaded(Scene scene)
        {
            Scene = scene;
            State = EResState.Unloaded;
            IsActive = false;
            Alive = false;
            _unloadAsyncOperation = null;

            // FDebug.Log($"OnSceneUnloaded: {SceneName}, state: {State}", LogTag.EasyFramework);
        }
        internal void OnActiveChanged(bool value)
        {
            IsActive = value;
        }
        
        sealed class Task : ETask.TaskAwaiter
        {
            private SceneInfo _info;
            private EResState _state;
            
            private bool IsCompleted => _info.State != _state;

            protected override bool OnTaskTick()
            {
                if (IsCompleted)
                {
                    TrySetResult();
                    return false;
                }
                return true;
            }
            
            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<Task>.Shared.Return(this);
            }

            public static Task Create(SceneInfo mainInfo, EResState state, out Guid token)
            {
                var task = ObjectPool<Task>.Shared.Rent();
                task._info = mainInfo;
                task._state = state;
                task.Start(out token);
                return task;
            }
        }
        
    }
}