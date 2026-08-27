// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public class DLCManager : Singleton<DLCManager>, IDLCManager
//     {
//         public enum EResult
//         {
//             Success,
//             AppVersionTooLow,
//             UpdateVersionError,
//             UpdateVersionInfoError,
//         }
//         public enum EState
//         {
//             None,
//             UpdateVersion,
//             UpdateVersionInfo,
//             Completed,
//         }
//
//         public EState State { get; private set; }
//
//         public string ServerUrl { get; private set; }
//         public DLCVersion Version { get; private set; }
//
//         private EasyFrameworkConfig Config => EasyFrameworkConfig.Instance;
//         private readonly Dictionary<string, string> _fileNameHashDict = new();
//         private readonly Dictionary<string, HashFileInfo> _fileInfoHashDict = new();
//
//         private Action<EResult> _callback = null;
//
//         
//         
//         
//         
//         
//         public void Enter(Action<EResult> callback = null)
//         {
//             Enter(EasyFrameworkConfig.Instance.dlcVersion, callback);
//         }
//         public void Enter(string dlcVersion, Action<EResult> callback = null)
//         {
//             if (State != EState.None || State == EState.Completed)
//             {
//                 Debug.LogError($"DLCManager is busying, current state: {State}");
//                 return;
//             }
//
//             _callback = callback;
//
//             if (string.IsNullOrEmpty(dlcVersion))
//             {
//                 EnterState(EState.UpdateVersion);
//             }
//             else
//             {
//                 Config.dlcVersion = dlcVersion;
//                 Config.Save();
//                     
//                 EnterState(EState.UpdateVersionInfo);
//             }
//         }
//
//         private void OnCompleted(EResult result)
//         {
//             State = EState.Completed;
//             _callback?.Invoke(result);
//             _callback = null;
//         }
//
//         private void EnterState(EState state)
//         {
//             State = state;
//             switch (state)
//             {
//                 case EState.UpdateVersion:
//                     UpdateVersion();
//                     break;
//                 case EState.UpdateVersionInfo:
//                     UpdateVersionInfo();
//                     break;
//             }
//         }
//
//         private void UpdateVersion()
//         {
//             if (EasyFrameworkSettings.App == null || string.IsNullOrEmpty(EasyFrameworkSettings.App.AppVersionFileUrl))
//             {
//                 FDebug.LogError($"AppVersionFileUrl is empty");
//                 OnCompleted(EResult.UpdateVersionError);
//                 return;
//             }
//
//             // var url = $"{EasyFrameworkSettings.App.AppVersionFileUrl}?{Guid.NewGuid()}";
//             var url = $"{EasyFrameworkSettings.App.AppVersionFileUrl}";
//             Debug.Log(url);
//             F.HttpManager.GetString(url, (b, s) =>
//             {
//                 Debug.Log($"{b}, {s}");
//                 if (b)
//                 {
//                     Version = JsonUtility.FromJson<DLCVersion>(s);
//                     if (Version.versionIndex > EasyFrameworkSettings.Instance.dlcVersionIndex)
//                     {
//                         OnCompleted(EResult.AppVersionTooLow);
//                         return;
//                     }
//
//                     Config.dlcVersion = Version.versionName;
//                     Config.Save();
//                     
//                     EnterState(EState.UpdateVersionInfo);
//                 }
//                 else
//                 {
//                     OnCompleted(EResult.UpdateVersionError);
//                 }
//             });
//         }
//
//         private void UpdateVersionInfo()
//         {
//             ServerUrl = $"{EasyFrameworkSettings.App.DLCPlatformServerUrl}/{Config.dlcVersion}/{EDLCMode.List}";
//
//             var localVersionInfo = F.LocalStorageManager.LoadObject<DLCVersionInfo>(DLCVersionInfo.FileName, ELocalStorageType.DLC);
//             if (Config.dlcVersionUid == Version.versionUid && localVersionInfo != null)
//             {
//                 UpdateHashInfos(localVersionInfo);
//                 OnCompleted(EResult.Success);
//                 return;
//             }
//             
//             var versionUrl = $"{ServerUrl}/{DLCVersionInfo.FileName}?{Guid.NewGuid()}";
//             Debug.Log(versionUrl);
//             F.HttpManager.GetString(versionUrl, (b, s) =>
//             {
//                 Debug.Log($"{b}, {s}");
//                 if (b)
//                 {
//                     var dlcVersionInfoServer = JsonUtility.FromJson<DLCVersionInfo>(s);
//                     if (dlcVersionInfoServer == null)
//                     {
//                         OnCompleted(EResult.UpdateVersionInfoError);
//                         return;
//                     }
//                     
//                     F.LocalStorageManager.SaveString(DLCVersionInfo.FileName, s, ELocalStorageType.DLC);
//                     Config.dlcVersionUid = Version.versionUid;
//                     Config.Save();
//
//                     if (localVersionInfo != null)
//                     {
//                         DeleteUnversionedFiles(localVersionInfo, dlcVersionInfoServer);
//                     }
//
//                     UpdateHashInfos(dlcVersionInfoServer);
//                     OnCompleted(EResult.Success);
//                 }
//                 else
//                 {
//                     OnCompleted(EResult.UpdateVersionInfoError);
//                 }
//             });
//         }
//
//         private void UpdateHashInfos(DLCVersionInfo versionInfo)
//         {
//             foreach (var info in versionInfo.hashFiles)
//             {
//                 if (!_fileInfoHashDict.TryAdd(info.fileName, info))
//                 {
//                     Debug.LogWarning($"UpdateHashFiles fileName: {info.fileName} already exists");
//                 }
//                 if (!_fileNameHashDict.TryAdd(info.fileName, info.hashFileName))
//                 {
//                     Debug.LogWarning($"UpdateHashFiles hashFileName: {info.hashFileName} already exists");
//                 }
//             }
//         }
//         
//         private void DeleteUnversionedFiles(DLCVersionInfo oldVersion, DLCVersionInfo newVersion)
//         {
//             var hashSet = newVersion.hashFiles.Select(item => item.hashFileName).ToHashSet();
//             foreach (var info in oldVersion.hashFiles)
//             {
//                 if (!hashSet.Contains(info.hashFileName))
//                 {
//                     F.LocalStorageManager.Delete(info.fileName, ELocalStorageType.DLC);
//                 }
//             }
//         }
//
//         public string GetFileHashName(string fileName) => _fileNameHashDict.GetValueOrDefault(fileName);
//         
//         public ETask<bool> DownloadFileAsync(string fileName)
//         {
//             if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
//                 return ETask.FromResult(false);
//
//             var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
//             var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
//             return F.HttpManager.DownloadFileAsync(downloadUrl, downloadFile);
//         }
//         public void DownloadFile(string fileName, Action<bool> callback = null)
//         {
//             if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
//             {
//                 FDebug.Log($"DownloadFile fileName: {fileName} not found");
//                 
//                 callback?.Invoke(false);
//                 return;
//             }
//
//             var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
//             var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
//             F.HttpManager.DownloadFile(downloadUrl, downloadFile, callback);
//         }
//         
//         public ETask<bool> DownloadFilesAsync(string[] fileNames)
//         {
//             HttpDownloadRequest[] requests = new HttpDownloadRequest[fileNames.Length];
//             for (int i = 0; i < fileNames.Length; i++)
//             {
//                 var fileName = fileNames[i];
//                 
//                 if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
//                     return ETask.FromResult(false);
//                 
//                 var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
//                 var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
//                 requests[i] = new HttpDownloadRequest(downloadUrl, downloadFile);
//             }
//             
//             return F.HttpManager.DownloadFilesAsync(requests);
//         }
//         public void DownloadFiles(string[] fileNames, Action<bool> callback = null)
//         {
//             HttpDownloadRequest[] requests = new HttpDownloadRequest[fileNames.Length];
//             for (int i = 0; i < fileNames.Length; i++)
//             {
//                 var fileName = fileNames[i];
//                 
//                 if (!_fileInfoHashDict.TryGetValue(fileName, out var hashFileInfo))
//                 {
//                     callback?.Invoke(false);
//                     return;
//                 }
//                 
//                 var downloadUrl = $"{ServerUrl}/{hashFileInfo.hashFileName}";
//                 var downloadFile = F.LocalStorageManager.GetFilePath(hashFileInfo.fileName, ELocalStorageType.DLC);
//                 requests[i] = new HttpDownloadRequest(downloadUrl, downloadFile);
//             }
//             F.HttpManager.DownloadFiles(requests, callback);
//         }
//     }
// }