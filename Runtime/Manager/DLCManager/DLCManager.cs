/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine.Networking;

namespace EasyFramework
{
    internal class DLCManager : Singleton<DLCManager>, IDLCManager
    {
        public enum EState
        {
            None,
            UpdateVersion,
            UpdateVersionInfo,
            Completed,
        }
        
        public EState State { get; private set; }
        public bool Initialized { get; private set; }
        
        private EasyFrameworkConfig Config => EasyFrameworkConfig.Instance;

        public async ETask InitializeAsync()
        {
#if UNITY_EDITOR
            if (EasyFrameworkSettings.Instance.resLoaderEditorMode)
            {
                return;
            }
#endif
            try
            {
                switch (EasyFrameworkSettings.Instance.resLoaderMode)
                {
                    case EResLoaderMode.DLC_StreamingAssets:
                    
                        var url = $"{EasyFrameworkSettings.Instance.StreamingAssetsDLCPath}/{DLCVersionInfo.FileName}";
                        await DLCUpdater.Instance.InitializeAsync(url);
                    
                        break;
                    case EResLoaderMode.DLC_CDN:
                    
                        await InitVersionAsync();
                        await DLCUpdater.Instance.InitializeAsync();
                    
                        break;
                }
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
        }

        public async ETask<IDLCManager.EResult> UpdateAsync()
        {
            var indexResult = await InitVersionAsync();
            if (!indexResult) return IDLCManager.EResult.InitVersionError;
            
            var updateResult = await DLCUpdater.Instance.UpdateAsync();
            if (updateResult != DLCUpdater.EResult.Success) return IDLCManager.EResult.DLCUpdaterError;
            
            return IDLCManager.EResult.Success;
        }
        
        public async ETask<bool> InitVersionAsync(string versionName = null)
        {
            var versionUrl = EasyFrameworkSettings.AppSettings.AppVersionURL;
            var webRequest = await ETask.UnityWebRequest(versionUrl);
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                FDebug.LogError($"AppVersionUrl: {versionUrl}\nresult: {webRequest.result}");
                return false;
            }
            FDebug.Log($"AppVersionUrl: {versionUrl}\n{webRequest.downloadHandler.text}");
            
            var dlcVersion = ConfigHelper.LoadFromText<DLCVersion>(webRequest.downloadHandler.text);
            if (dlcVersion == null)
            {
                FDebug.LogError($"DLCManager init dlcVersion is null");
                return false;
            }

            var localIndex = EasyFrameworkSettings.Instance.dlcVersionIndex;
            if (dlcVersion.versionIndex > localIndex)
            {
                FDebug.LogError($"DLCManager init failed because version too low. {localIndex} - {dlcVersion.versionIndex}");
                return false;
            }
            
            Config.dlcVersion = dlcVersion;
            Config.Save();

            return true;
        }

        public string GetFileName(string resName) => DLCUpdater.Instance.GetFileName(resName);
        public string GetFilePath(string resName) => DLCUpdater.Instance.GetFilePath(resName);
        public bool Exists(string resName) => DLCUpdater.Instance.Exists(resName);
        public ETask<bool> DownloadAsync(string resName) => DLCUpdater.Instance.DownloadAsync(resName);
        public ETask<bool> DownloadAsync(string resName, out string filePath)
        {
            filePath = DLCUpdater.Instance.GetFilePath(resName);
            return DLCUpdater.Instance.DownloadAsync(resName);
        }
        public ETask<string> DownloadAndReturnFileAsync(string resName) => DLCUpdater.Instance.DownloadAndReturnFileAsync(resName);
    }
}