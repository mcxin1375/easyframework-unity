using System.IO;
using System.Threading;
using UnityEngine;

namespace EasyFramework
{
    public enum MainResManagerResult
    {
        Success,
        LoadFromStreamingAssetsError,
        UnzipError,
    }
    
    public class MainResManager : Singleton<MainResManager>
    {
        public float Progress
        {
            get
            {
                return UnzipManager.Instance.Progress;
            }
        }
        private float _downloadProgress;

        private StreamingAssetsResZipInfo ResZipInfo => EasyFrameworkSettings.Instance.streamingAssetsResZipInfo;

        public bool IsMainResLoaded 
        {
            get
            {
                var easyMainConfig = EasyFrameworkConfig.Instance;
                return easyMainConfig.mainResUid == ResZipInfo.mainResUid;
            }
        }

        public async ETask<MainResManagerResult> EnterAsync(CancellationToken cancellationToken = default)
        {
            var easyMainConfig = EasyFrameworkConfig.Instance;
            var mainResInfo = EasyFrameworkSettings.Instance.streamingAssetsResZipInfo;
            Debug.Log($"MainRes UID: {easyMainConfig.mainResUid} - {mainResInfo.mainResUid}");
            
            if (IsMainResLoaded) return MainResManagerResult.Success;
            
            if (mainResInfo.mainResZipArray == null || mainResInfo.mainResZipArray.Length == 0)
            {
                F.LocalStorageManager.ClearDirectory(ELocalStorageType.DLC);
                UpdateConfig();
                return MainResManagerResult.Success;
            }

            var mainResPath = Application.streamingAssetsPath;
            if (Application.platform == RuntimePlatform.Android || Application.isEditor)
            {
                mainResPath = F.LocalStorageManager.GetDirectoryPath(ELocalStorageType.DownloadTemp);

                var result = await DownloadMainResFromLocalAsync(cancellationToken);
                if (!result) return MainResManagerResult.LoadFromStreamingAssetsError;
            }
            
            var zipResult = await UnzipMainResAsync(mainResPath, cancellationToken);
            if (!zipResult) return MainResManagerResult.UnzipError;
            
            UpdateConfig();
            return MainResManagerResult.Success;
        }

        private void UpdateConfig()
        {
            var config = EasyFrameworkConfig.Instance;
            config.mainResUid = ResZipInfo.mainResUid;
            config.Save();
        }

        private async ETask<bool> DownloadMainResFromLocalAsync(CancellationToken cancellationToken = default)
        {
            // Debug.Log("DownloadMainResFromLocalAsync");

            _downloadProgress = 0;

            var downloadUrl = Application.streamingAssetsPath;
            var downloadPath = F.LocalStorageManager.GetDirectoryPath(ELocalStorageType.DownloadTemp);
            if (ResZipInfo.mainResZipArray?.Length > 0)
            {
                // float rate = 1 / (float)mainResInfo.mainResZipArray.Length;
                for (int i = 0; i < ResZipInfo.mainResZipArray.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var resInfo = ResZipInfo.mainResZipArray[i];
                    string fromFile = $"{downloadUrl}/{resInfo.name}";
                    string toFile = $"{downloadPath}/{resInfo.name}";
                    
                    var result = await F.HttpManager.DownloadFileAsync(fromFile, toFile, cancellationToken);
                    if (!result)
                    {
                        FDebug.LogError("DownloadFileAsync Failed: " + resInfo.name);
                        return false;
                    }

                    _downloadProgress = (float)(i + 1) / ResZipInfo.mainResZipArray.Length;
                }
            }

            return true;
        }

        private async ETask<bool> UnzipMainResAsync(string downloadPath, CancellationToken cancellationToken = default)
        {
            // Debug.Log("UnzipMainResAsync");
            
            F.LocalStorageManager.ClearDirectory(ELocalStorageType.DLC);
            
            if (ResZipInfo.mainResZipArray?.Length > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                for (int i = 0; i < ResZipInfo.mainResZipArray.Length; i++)
                {
                    var resInfo = ResZipInfo.mainResZipArray[i];
                    string unzipFile = $"{downloadPath}/{resInfo.name}";
                    UnzipManager.Instance.AddRequest(unzipFile, F.LocalStorageManager.GetDirectoryPath(ELocalStorageType.DLC));
                }

                // List<UnzipRequest> tmpList = new();
                // tmpList.Add(requests[0]);
                // tmpList.Add(requests[0]);
                // var result = await _dlcCompressor.DoAsync(tmpList.ToArray());
                
                var result = await UnzipManager.Instance.DoAsync(cancellationToken);
                if (!result)
                {
                    return false;
                }
            }
            
            F.LocalStorageManager.ClearDirectory(ELocalStorageType.DownloadTemp);
            
            return true;
        }
    }
}