/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        
        public DLCVersionInfo DLCVersionInfo => DLCUpdater.Instance.VersionInfo;

        private EasyFrameworkConfig Config => EasyFrameworkConfig.Instance;

        public async ETask InitializeAsync()
        {
#if UNITY_EDITOR
            if (EasyFrameworkSettings.Instance.resLoaderEditorMode)
            {
                return;
            }
#endif
            var result = await IndexVersionAsync();
        }

        public async ETask<IDLCManager.EResult> UpdateAsync()
        {
            var indexResult = await IndexVersionAsync();
            if (!indexResult) return IDLCManager.EResult.IndexVersionError;
            
            var updateResult = await DLCUpdater.Instance.UpdateAsync();
            if (updateResult != DLCUpdater.EResult.Success) return IDLCManager.EResult.DLCUpdaterError;
            
            return IDLCManager.EResult.Success;
        }
        
        public async ETask<bool> IndexVersionAsync(string versionName = null)
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
            
            await DLCUpdater.Instance.InitializeAsync();

            return true;
        }

        public string GetResFilePath(string resName) => DLCUpdater.Instance.GetResFilePath(resName);

        public ETask<bool> DownloadAsync(string resName) => DLCUpdater.Instance.DownloadAsync(resName);
        public ETask<string> DownloadAndReturnFileAsync(string resName) => DLCUpdater.Instance.DownloadAndReturnFileAsync(resName);

        public void DownloadFile(string fileName, Action<bool> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DownloadFiles(string[] fileNames, Action<bool> callback = null)
        {
            throw new NotImplementedException();
        }

        public ETask<bool> DownloadFileAsync(string fileName)
        {
            throw new NotImplementedException();
        }
        // public void Enter(Action<EResult> callback = null)
        // {
        //     Enter(EasyFrameworkConfig.Instance.dlcVersion, callback);
        // }
        // public void Enter(string dlcVersion, Action<EResult> callback = null)
        // {
        //     if (State != EState.None || State == EState.Completed)
        //     {
        //         Debug.LogError($"DLCManager is busying, current state: {State}");
        //         return;
        //     }
        //
        //     _callback = callback;
        //
        //     if (string.IsNullOrEmpty(dlcVersion))
        //     {
        //         EnterState(EState.UpdateVersion);
        //     }
        //     else
        //     {
        //         Config.dlcVersion = dlcVersion;
        //         Config.Save();
        //             
        //         EnterState(EState.UpdateVersionInfo);
        //     }
        // }
        //
        // private void OnCompleted(EResult result)
        // {
        //     State = EState.Completed;
        //     _callback?.Invoke(result);
        //     _callback = null;
        // }
        //
        // private void EnterState(EState state)
        // {
        //     State = state;
        //     switch (state)
        //     {
        //         case EState.UpdateVersion:
        //             UpdateVersion();
        //             break;
        //         case EState.UpdateVersionInfo:
        //             UpdateVersionInfo();
        //             break;
        //     }
        // }
        //
        // private void UpdateVersion()
        // {
        //     if (EasyFrameworkSettings.App == null || string.IsNullOrEmpty(EasyFrameworkSettings.App.AppVersionFileUrl))
        //     {
        //         FDebug.LogError($"AppVersionFileUrl is empty");
        //         OnCompleted(EResult.UpdateVersionError);
        //         return;
        //     }
        //
        //     // var url = $"{EasyFrameworkSettings.App.AppVersionFileUrl}?{Guid.NewGuid()}";
        //     var url = $"{EasyFrameworkSettings.App.AppVersionFileUrl}";
        //     Debug.Log(url);
        //     F.HttpManager.GetString(url, (s) =>
        //     {
        //         Debug.Log($"{s}");
        //         try
        //         {
        //             Version = JsonUtility.FromJson<DLCVersion>(s);
        //             if (Version.versionIndex > EasyFrameworkSettings.Instance.dlcVersionIndex)
        //             {
        //                 OnCompleted(EResult.AppVersionTooLow);
        //                 return;
        //             }
        //
        //             Config.dlcVersion = Version.versionName;
        //             Config.Save();
        //             
        //             EnterState(EState.UpdateVersionInfo);
        //         }
        //         catch (Exception e)
        //         {
        //             FDebug.LogException(e);
        //             OnCompleted(EResult.UpdateVersionError);
        //         }
        //     });
        // }
        //
        // private void UpdateVersionInfo()
        // {
        //     ServerUrl = $"{EasyFrameworkSettings.App.DLCPlatformServerUrl}/{Config.dlcVersion}/{EDLCMode.List}";
        //
        //     var localVersionInfo = F.LocalStorageManager.LoadObject<DLCVersionInfo>(DLCVersionInfo.FileName, ELocalStorageType.DLC);
        //     if (Config.dlcVersionUid == Version.versionUid && localVersionInfo != null)
        //     {
        //         UpdateHashInfos(localVersionInfo);
        //         OnCompleted(EResult.Success);
        //         return;
        //     }
        //     
        //     var versionUrl = $"{ServerUrl}/{DLCVersionInfo.FileName}?{Guid.NewGuid()}";
        //     Debug.Log(versionUrl);
        //     F.HttpManager.GetString(versionUrl, (s) =>
        //     {
        //         Debug.Log($"{s}");
        //         
        //         try
        //         {
        //             var dlcVersionInfoServer = JsonUtility.FromJson<DLCVersionInfo>(s);
        //             if (dlcVersionInfoServer == null)
        //             {
        //                 OnCompleted(EResult.UpdateVersionInfoError);
        //                 return;
        //             }
        //             
        //             F.LocalStorageManager.SaveString(DLCVersionInfo.FileName, s, ELocalStorageType.DLC);
        //             Config.dlcVersionUid = Version.versionUid;
        //             Config.Save();
        //
        //             if (localVersionInfo != null)
        //             {
        //                 DeleteUnversionedFiles(localVersionInfo, dlcVersionInfoServer);
        //             }
        //
        //             UpdateHashInfos(dlcVersionInfoServer);
        //             OnCompleted(EResult.Success);
        //         }
        //         catch (Exception e)
        //         {
        //             FDebug.LogException(e);
        //             OnCompleted(EResult.UpdateVersionInfoError);
        //         }
        //     });
        // }
        //
        // private void UpdateHashInfos(DLCVersionInfo versionInfo)
        // {
        //     foreach (var info in versionInfo.hashFiles)
        //     {
        //         if (!_fileInfoHashDict.TryAdd(info.fileName, info))
        //         {
        //             Debug.LogWarning($"UpdateHashFiles fileName: {info.fileName} already exists");
        //         }
        //         if (!_fileNameHashDict.TryAdd(info.fileName, info.hashFileName))
        //         {
        //             Debug.LogWarning($"UpdateHashFiles hashFileName: {info.hashFileName} already exists");
        //         }
        //     }
        // }
        //
        // private void DeleteUnversionedFiles(DLCVersionInfo oldVersion, DLCVersionInfo newVersion)
        // {
        //     var hashSet = newVersion.hashFiles.Select(item => item.hashFileName).ToHashSet();
        //     foreach (var info in oldVersion.hashFiles)
        //     {
        //         if (!hashSet.Contains(info.hashFileName))
        //         {
        //             F.LocalStorageManager.Delete(info.fileName, ELocalStorageType.DLC);
        //         }
        //     }
        // }
        //
        // public string GetFileHashName(string fileName) => _fileNameHashDict.GetValueOrDefault(fileName);
        //
        // public ETask<bool> DownloadFileAsync(string fileName)
        // {
        //     if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
        //         return ETask.FromResult(false);
        //
        //     var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
        //     var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
        //     return F.HttpManager.DownloadFileAsync(downloadUrl, downloadFile);
        // }
        // public void DownloadFile(string fileName, Action<bool> callback = null)
        // {
        //     if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
        //     {
        //         FDebug.Log($"DownloadFile fileName: {fileName} not found");
        //         
        //         callback?.Invoke(false);
        //         return;
        //     }
        //
        //     var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
        //     var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
        //     F.HttpManager.DownloadFile(downloadUrl, downloadFile, callback);
        // }
        //
        // public ETask<bool> DownloadFilesAsync(string[] fileNames)
        // {
        //     HttpDownloadRequest[] requests = new HttpDownloadRequest[fileNames.Length];
        //     for (int i = 0; i < fileNames.Length; i++)
        //     {
        //         var fileName = fileNames[i];
        //         
        //         if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
        //             return ETask.FromResult(false);
        //         
        //         var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
        //         var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
        //         requests[i] = new HttpDownloadRequest(downloadUrl, downloadFile);
        //     }
        //     
        //     return F.HttpManager.DownloadFilesAsync(requests);
        // }
        // public void DownloadFiles(string[] fileNames, Action<bool> callback = null)
        // {
        //     HttpDownloadRequest[] requests = new HttpDownloadRequest[fileNames.Length];
        //     for (int i = 0; i < fileNames.Length; i++)
        //     {
        //         var fileName = fileNames[i];
        //         
        //         if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
        //         {
        //             callback?.Invoke(false);
        //             return;
        //         }
        //         
        //         var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
        //         var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
        //         requests[i] = new HttpDownloadRequest(downloadUrl, downloadFile);
        //     }
        //     F.HttpManager.DownloadFiles(requests, callback);
        // }
    }
}