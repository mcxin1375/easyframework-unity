/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EasyFramework
{
    internal class AssetBundleLoader : Singleton<AssetBundleLoader>, ITickerNode, IResLoader
    {
        public AssetBundleManifest Manifest { get; private set; }

        internal IReadOnlyDictionary<string, AssetBundleInfo> AbDict => _abDict;
        internal IReadOnlyDictionary<string, AssetBundleRequestHandler> RequestDict => _requestDict;
        
        private readonly Dictionary<string, AssetBundleInfo> _keyDict = new();
        private readonly Dictionary<string, AssetBundleInfo> _abDict = new();
        private readonly Dictionary<string, AssetBundleRequestHandler> _requestDict = new();
        private readonly Queue<AssetBundleRequestHandler> _unloadQueue = new();
        private readonly List<AssetBundleInfo> _tmpList = new();

        public AssetBundleLoader()
        {
            ObjectPool<AssetBundleRequestHandler>.Shared.CreateFunc = () => new AssetBundleRequestHandler();
            ETask.AddTick(this);
        }

        async ETask IResLoader.PreInitializeAsync()
        {
            if (!F.LocalStorageManager.Exists(AssetBundleManifest.FileName, ELocalStorageType.DLC))
            {
                var result = await F.DLCDownloader.DownloadAsync(AssetBundleManifest.FileName);
                if (!result)
                {
                    FDebug.LogError($"DLCDownloader download {AssetBundleManifest.FileName} error!");
                    return;
                }
            }

            var content = F.LocalStorageManager.ReadAllText(AssetBundleManifest.FileName, ELocalStorageType.DLC);
            Manifest = ConfigHelper.LoadFromText<AssetBundleManifest>(content);
            
            foreach (var kv in Manifest.depDict)
            {
                _abDict.Add(kv.Key, new AssetBundleInfo(kv.Key, kv.Value));
            }
        }

        bool ITickerNode.OnTick()
        {
            foreach (var request in _requestDict.Values)
            {
                if (!request.Alive) _unloadQueue.Enqueue(request);
            }

            if (_unloadQueue.Count > 0)
            {
                while (_unloadQueue.Count > 0)
                {
                    var request = _unloadQueue.Dequeue();
                    _requestDict.Remove(request.AbName);
                    
                    request.UnloadForce(false);
                    ObjectPool<AssetBundleRequestHandler>.Shared.Return(request);
                }
            }

            return true;
        }

        public AssetBundle Load(string abName, IResRequest handler = null)
        {
            // Log.Info($"------------------------------------ Load: {abName}");

            if (!_requestDict.TryGetValue(abName, out var request))
            {
                if (!TryGetOrCreate(abName, out var mainInfo)) return null;
                
                // request = ObjectPoolItem<AssetBundleRequestHandler>.Shared.Rent();
                request = new();
                _requestDict.Add(abName, request);

                request.InitInfo(abName, mainInfo);
            }
            
            return request.Load(handler);
        }

        public async ETask<AssetBundle> LoadAsync(string abName, IResRequest handler = null)
        {
            // Log.Info($"------------------------------------ LoadAsync: {abName}");

            if (!_requestDict.TryGetValue(abName, out var request))
            {
                if (!TryGetOrCreate(abName, out var mainInfo)) return null;
                
                // request = ObjectPoolItem<AssetBundleRequestHandler>.Shared.Rent();
                request = new();
                _requestDict.Add(abName, request);

                request.InitInfo(abName, mainInfo);
            }
            
            return await request.LoadAsync(handler);
        }

        public void Unload(string abName, IResRequest handler = null)
        {
            if (!_requestDict.TryGetValue(abName, out var request)) return;
            if (request.Unload(handler, false))
            {
                _requestDict.Remove(abName);
                ObjectPool<AssetBundleRequestHandler>.Shared.Return(request);
            }
        }
        
        public void UnloadForce(string abName, bool unloadAllLoadedObjects = false)
        {
            if (!_requestDict.TryGetValue(abName, out var request)) return;
            request.UnloadForce(unloadAllLoadedObjects);
            _requestDict.Remove(abName);
            ObjectPool<AssetBundleRequestHandler>.Shared.Return(request);
        }

        public void UnloadAllForce(bool unloadAllLoadedObjects = false)
        {
            foreach (var request in _requestDict.Values)
            {
                request.UnloadForce(unloadAllLoadedObjects);
                ObjectPool<AssetBundleRequestHandler>.Shared.Return(request);
            }
            _requestDict.Clear();
        }

        public bool IsLoading(string abName)
        {
            if (!TryGetOrCreate(abName, out var mainInfo)) return false;
            if (mainInfo.IsLoading) return true;
            if (mainInfo.Dependencies?.Length > 0)
            {
                foreach (var depInfo in mainInfo.Dependencies)
                {
                    if (depInfo.IsLoading) return true;
                }
            }

            return false;
        }

        public bool IsUnloading(string abName)
        {
            if (!TryGetOrCreate(abName, out var mainInfo)) return false;
            if (mainInfo.IsUnloading) return true;
            if (mainInfo.Dependencies?.Length > 0)
            {
                foreach (var depInfo in mainInfo.Dependencies)
                {
                    if (depInfo.IsUnloading) return true;
                }
            }

            return false;
        }
        public float GetLoadingProgress(string abName)
        {
            if (!TryGetOrCreate(abName, out var mainInfo)) return 0;

            float p = mainInfo.LoadingProgress;
            float len = 1;
            if (mainInfo.Dependencies?.Length > 0)
            {
                len += mainInfo.Dependencies.Length;
                foreach (var depInfo in mainInfo.Dependencies)
                {
                    p += depInfo.LoadingProgress;
                }
            }
            return p / len;
        }

        public string[] GetAllDependencies(string abName) => Manifest?.GetAllDependencies(abName);
        
        
        public T LoadAsset<T>(string abName, IResRequest request = null) where T : Object
        {
            var ab = Load(abName, request);
            return ab?.LoadAsset<T>(abName);
        }
        public T[] LoadAllAssets<T>(string abName, IResRequest request = null) where T : Object
        {
            var ab = Load(abName, request);
            return ab?.LoadAllAssets<T>();
        }
        public async ETask<T> LoadAssetAsync<T>(string abName, IResRequest request = null) where T : Object
        {
            var ab = await LoadAsync(abName, request);
            if (ab == null) return null;
            return await ab.LoadAssetAsyncEx<T>(abName)!;
        }
        public async ETask<T[]> LoadAllAssetsAsync<T>(string abName, IResRequest request = null) where T : Object
        {
            var ab = await LoadAsync(abName, request);
            if (ab == null) return Array.Empty<T>();
            return await ab.LoadAllAssetsAsyncEx<T>();
        }
        
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IResRequest handler = null)
        {
            var ab = Load(sceneName, handler);
            if (ab == null)
            {
                FDebug.LogError($"Load scene[{sceneName}] error! because assetBundle is empty!");
                return;
            }

            SceneManager.LoadScene(sceneName, mode);
        }
        public async ETask<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IResRequest handler = null)
        {
            var ab = await LoadAsync(sceneName, handler);
            if (ab == null)
            {
                FDebug.LogError($"Load scene[{sceneName}] error! because assetBundle is empty!");
                return null;
            }

            return SceneManager.LoadSceneAsync(sceneName, mode);
        }
        public ETask<AsyncOperation> UnloadSceneAsync(string sceneName, IResRequest handler = null)
        {
            Unload(sceneName, handler);
            var result = SceneManager.UnloadSceneAsync(sceneName);
            return ETask.FromResult(result);
        }
        
        private bool TryGetOrCreate(string abName, out AssetBundleInfo mainInfo)
        {
            if (!_keyDict.TryGetValue(abName, out mainInfo))
            {
                var nameKey = AssetBundleHelper.NameToKey(abName);
                if (!_abDict.TryGetValue(nameKey, out mainInfo)) return false;

                _tmpList.Clear();
                var dependencies = Manifest.GetAllDependencies(mainInfo.FileName);
                if (dependencies?.Length > 0)
                {
                    foreach (string depName in dependencies)
                    {
                        if (!_abDict.TryGetValue(depName, out var depInfo)) continue;
                        if (mainInfo == depInfo) continue;
                        _tmpList.Add(depInfo);
                    }
                }
                mainInfo.Dependencies = _tmpList.ToArray();
                
                _keyDict[abName] = mainInfo;
            }
            return true;
        }
    }
}