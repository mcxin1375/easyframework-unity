/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/

using System.IO;
using UnityEngine;

namespace EasyFramework
{
    public class AssetBundleInfo : ITickerNode
    {
        public bool IsLoading => State == AssetBundleState.Loading;
        public bool IsUnloading => State == AssetBundleState.Unloading;

        public float LoadingProgress
        {
            get
            {
                switch (State)
                {
                    case AssetBundleState.Loading: return _assetBundleCreateRequest?.progress ?? 0;
                    default: return Bundle == null ? 0 : 1;
                }
            }
        }

        public float UnloadingProgress 
        {
            get
            {
                switch (State)
                {
                    case AssetBundleState.Unloading: return _assetBundleUnloadOperation?.progress ?? 1;
                    default: return Bundle == null ? 1 : 0;
                }
            }
        }

        public readonly string FileName;
        public readonly string FilePath;
        public readonly string[] DepNames;
        public AssetBundleInfo[] Dependencies;
        public int RefCount;
        public AssetBundleState State { get; private set; } = AssetBundleState.None;
        public AssetBundle Bundle { get; private set; }
        private AssetBundleCreateRequest _assetBundleCreateRequest;
        private AssetBundleUnloadOperation _assetBundleUnloadOperation;
        private bool _downloading;

        public AssetBundleInfo(string abName, string[] abDepNames)
        {
            FileName = abName;
            DepNames = abDepNames;
            FilePath = AssetBundleHelper.NameToURL(FileName);
        }

        bool ITickerNode.OnTick()
        {
            switch (State)
            {
                case AssetBundleState.Loading:

                    if (_assetBundleCreateRequest == null)
                    {
                        State = AssetBundleState.None;
                    }
                    else if (_assetBundleCreateRequest.isDone)
                    {
                        Bundle = _assetBundleCreateRequest.assetBundle;
                        _assetBundleCreateRequest = null;
                        State = AssetBundleState.None;
                        // Debug.Log($"AssetBundle[{FileName}] loading done!");
                    }

                    return true;
                case AssetBundleState.Unloading:

                    if (_assetBundleUnloadOperation == null)
                    {
                        State = AssetBundleState.None;
                    }
                    else if (_assetBundleUnloadOperation.isDone)
                    {
                        Bundle = null;
                        _assetBundleUnloadOperation = null;
                        State = AssetBundleState.None;
                    }

                    return true;
            }

            return false;
        }

        internal AssetBundle Load()
        {
            // FDebug.Log($"AssetBundle [{Name}] Load()");
            
            if (Bundle != null) return Bundle;

            if (!File.Exists(FilePath))
            {
                FDebug.LogError($"AssetBundle[{FileName}] not exists: {FilePath}");
                return null;
            }

            switch (State)
            {
                case AssetBundleState.Loading:
                    if (_assetBundleCreateRequest != null)
                    {
                        _assetBundleCreateRequest.assetBundle.Unload(false);
                        _assetBundleCreateRequest = null;
                    }
                    break;
                case AssetBundleState.Unloading:
                    _assetBundleUnloadOperation.WaitForCompletion();
                    _assetBundleUnloadOperation = null;
                    break;
            }
            State = AssetBundleState.None;
            Bundle = AssetBundle.LoadFromFile(FilePath);
            return Bundle;
        }
        internal void LoadAsync()
        {
            // FDebug.Log($"AssetBundle [{Name}] LoadAsync()");
            
            if (Bundle != null || State == AssetBundleState.Loading) return;
            
            switch (State)
            {
                case AssetBundleState.Unloading:
                    _assetBundleUnloadOperation.WaitForCompletion();
                    _assetBundleUnloadOperation = null;
                    Bundle = null;
                    break;
            }
            
            if (!File.Exists(FilePath))
            {
                if (!_downloading)
                {
                    State = AssetBundleState.Loading;
                    _ = DownloadAsync();
                }
                return;
            }
            
            _assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(FilePath);
            State = _assetBundleCreateRequest != null ? AssetBundleState.Loading : AssetBundleState.None;

            if (State == AssetBundleState.Loading)
                ETask.AddTick(this);
        }

        private async ETask DownloadAsync()
        {
            if (_downloading) return;
            _downloading = true;

            await F.DLCDownloader.DownloadAsync(FileName);
            
            _downloading = false;

            if (!File.Exists(FilePath))
            {
                FDebug.LogError($"AssetBundle[{FileName}] download failed!");
                if (State == AssetBundleState.Loading) State = AssetBundleState.None;
                return;
            }
            
            if (State == AssetBundleState.Loading)
            {
                _assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(FilePath);
                State = _assetBundleCreateRequest != null ? AssetBundleState.Loading : AssetBundleState.None;

                if (State == AssetBundleState.Loading)
                    ETask.AddTick(this);
            }
        }

        internal void Unload(bool unloadAllLoadedObjects)
        {
            // FDebug.Log($"AssetBundle [{Name}] Unload(unloadAllLoadedObjects:{unloadAllLoadedObjects})", LogTag.EasyFramework);

            switch (State)
            {
                case AssetBundleState.Loading:
                    if (_assetBundleCreateRequest != null)
                    {
                        _assetBundleCreateRequest.assetBundle.Unload(unloadAllLoadedObjects);
                        _assetBundleCreateRequest = null;
                    }
                    break;
                case AssetBundleState.Unloading:
                    _assetBundleUnloadOperation.WaitForCompletion();
                    _assetBundleUnloadOperation = null;
                    break;
            }

            State = AssetBundleState.None;
            if (Bundle != null)
            {
                Bundle.Unload(unloadAllLoadedObjects);
                Bundle = null;
            }
        }
        internal void UnloadAsync(bool unloadAllLoadedObjects)
        {
            // FDebug.Log($"AssetBundle [{Name}] UnloadAsync(unloadAllLoadedObjects:{unloadAllLoadedObjects})", LogTag.EasyFramework);

            switch (State)
            {
                case AssetBundleState.None:
                    if (Bundle != null)
                    {
                        _assetBundleUnloadOperation = Bundle.UnloadAsync(unloadAllLoadedObjects);
                        State = _assetBundleUnloadOperation != null ? AssetBundleState.Unloading : AssetBundleState.None;
                    }
                    break;
                case AssetBundleState.Loading:
                    if (_assetBundleCreateRequest != null)
                    {
                        _assetBundleCreateRequest.assetBundle.Unload(unloadAllLoadedObjects);
                        _assetBundleCreateRequest = null;
                    }

                    State = AssetBundleState.None;
                    break;
            }

            if (State == AssetBundleState.Unloading)
                ETask.AddTick(this);
        }

        public enum AssetBundleState
        {
            None = 0,
            Loading = 1,
            Unloading = 2,
        }
    }
}