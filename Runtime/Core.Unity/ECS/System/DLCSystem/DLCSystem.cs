// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/11/28
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Cysharp.Threading.Tasks;
// using Newtonsoft.Json;
// using UnityEngine;
// using UnityEngine.Networking;
//
// namespace EasyFramework.Core
// {
//     public class DLCUpdateSystem : FSystem, IDLCManager
//     {
//         public ELogicState State { get; private set; } = ELogicState.None;
//         public float Progress { get; private set; }
//         public long DownloadedBytes { get; private set; }
//         public long DownloadTotalBytes { get; private set; }
//
//         private DLCUpdateRequest _dlcUpdateRequest;
//         private ELogicState _enterState = ELogicState.None;
//         private int _errorRetryCount;
//         private DLCDownloader Downloader { get; } = new DLCDownloader();
//         private DLCCompressor Compressor { get; } = new DLCCompressor();
//
//         protected override void OnUpdate()
//         {
//             base.OnUpdate();
//             
//                 if (_enterState != ELogicState.None)
//                 {
//                     State = _enterState;
//                     _enterState = ELogicState.None;
//                     Debug.Log($"{GetType().Name}: {State}");
//                     switch (State)
//                     {
//                         case ELogicState.DLCDownloadRes:
//                             F.CoroutineSystem.StartCoroutine(DLCDownloadRes());
//                             break;
//                         case ELogicState.DLCUnzipRes:
//                             F.CoroutineSystem.StartCoroutine(DLCUnzipRes());
//                             break;
//                         case ELogicState.Completed:
//                             break;
//                         case ELogicState.Error:
//                             break;
//                     }
//                 }
//         }
//
//
//         public async UniTask Enter()
//         {
//             List<string> packageList = new();
//             if (EasyFrameworkSettings.Instance.dlcBuiltinPackages.Count > 0) packageList.AddRange(EasyFrameworkSettings.Instance.dlcBuiltinPackages);
//             var config = MainLocalConfig.Instance;
//             foreach (var package in config.PackageList)
//             {
//                 if (!packageList.Contains(package)) packageList.Add(package);
//             }
//
//             var dlcUpdateRequest = await CreateDLCUpdateRequest(packageList.ToArray());
//             await Enter(dlcUpdateRequest);
//         }
//
//         public async UniTask Enter(DLCUpdateRequest dlcUpdateRequest)
//         {
//             State = ELogicState.None;
//             Progress = 0;
//             DownloadedBytes = 0;
//             DownloadTotalBytes = 0;
//             _errorRetryCount = 0;
//             _dlcUpdateRequest = dlcUpdateRequest;
//             _enterState = ELogicState.DLCDownloadRes;
//             
//             await UniTask.WaitUntil(() => State == ELogicState.Completed || State == ELogicState.Error || State == ELogicState.MainVersionTooLow);
//         }
//
//         public async UniTask WaitEnterCompleted()
//         {
//             await UniTask.WaitUntil(() => State == ELogicState.Completed || State == ELogicState.Error || State == ELogicState.MainVersionTooLow);
//         }
//
//         public async UniTask<DLCUpdateComponent> CreateDLCPackageDownloader(string package)
//         {
//             if (string.IsNullOrWhiteSpace(EasyFrameworkSettings.ISettings.DLCServerUrl)) return null;
//             
//             var config = MainLocalConfig.Instance;
//             var dlcVersionUrl = $"{EasyFrameworkSettings.ISettings.DLCServerUrl}/{config.DLCVersionVer}/{nameof(DLCVersion)}.json";
//             var dlcVersionServer = await DownloadDLCVersion(dlcVersionUrl);
//             
//             if (dlcVersionServer.mainVersion != EasyFrameworkSettings.ISettings.MainVersion) return null;
//
//             foreach (var packageName in dlcVersionServer.packages)
//             {
//                 if (package != packageName) continue;
//
//                 var packageVersionUrl = $"{EasyFrameworkSettings.ISettings.DLCServerUrl}/{dlcVersionServer.version}/{packageName}.json";
//                 var dlcPackageVersion = await DownloadPackageVersion(packageVersionUrl);
//                 var dlcPackageDownloader = await DLCUpdateComponent.CreateAsync(dlcVersionServer, dlcPackageVersion);
//                 return dlcPackageDownloader;
//             }
//             return null;
//         }
//         
//         public async UniTask<DLCUpdateComponent[]> CreateDLCPackageDownloaders(params string[] packages)
//         {
//             if (string.IsNullOrWhiteSpace(EasyFrameworkSettings.ISettings.DLCServerUrl)) return null;
//             
//             var config = MainLocalConfig.Instance;
//             var dlcVersionUrl = $"{EasyFrameworkSettings.ISettings.DLCServerUrl}/{config.DLCVersionVer}/{nameof(DLCVersion)}.json";
//             var dlcVersionServer = await DownloadDLCVersion(dlcVersionUrl);
//             
//             if (dlcVersionServer.mainVersion != EasyFrameworkSettings.ISettings.MainVersion) return null;
//
//             List<DLCUpdateComponent> downloaders = new();
//             foreach (var packageName in dlcVersionServer.packages)
//             {
//                 if (!packages.Contains(packageName)) continue;
//
//                 var packageVersionUrl = $"{EasyFrameworkSettings.ISettings.DLCServerUrl}/{dlcVersionServer.version}/{packageName}.json";
//                 var dlcPackageVersion = await DownloadPackageVersion(packageVersionUrl);
//                 var dlcPackageDownloader = await DLCUpdateComponent.CreateAsync(dlcVersionServer, dlcPackageVersion);
//                 
//                 downloaders.Add(dlcPackageDownloader);
//             }
//             return downloaders.ToArray();
//         }
//         
//         public async UniTask<DLCUpdateRequest> CreateDLCUpdateRequest(params string[] packages)
//         {
//             if (string.IsNullOrWhiteSpace(EasyFrameworkSettings.ISettings.DLCServerUrl)) return null;
//             
//             var config = MainLocalConfig.Instance;
//             var dlcVersionUrl = $"{EasyFrameworkSettings.ISettings.DLCServerUrl}/{config.DLCVersionVer}/{nameof(DLCVersion)}.json";
//             var dlcVersionServer = await DownloadDLCVersion(dlcVersionUrl);
//             
//             if (dlcVersionServer.mainVersion != EasyFrameworkSettings.ISettings.MainVersion) return null;
//
//             List<DLCPackageVersion> updatePackageList = new();
//             foreach (var packageName in dlcVersionServer.packages)
//             {
//                 if (!packages.Contains(packageName)) continue;
//
//                 var packageVersionUrl = $"{EasyFrameworkSettings.ISettings.DLCServerUrl}/{dlcVersionServer.version}/{packageName}.json";
//                 var dlcPackageVersion = await DownloadPackageVersion(packageVersionUrl);
//                 updatePackageList.Add(dlcPackageVersion);
//             }
//             
//             var dlcUpdateRequest = await DLCUpdateRequest.CreateAsync(dlcVersionServer, updatePackageList.ToArray(), (p) =>
//             {
//                 Progress = p;
//             });
//             Debug.Log($"downloadList: {dlcUpdateRequest.DownloadRequests.Length}, unzipList: {dlcUpdateRequest.UnzipRequests.Length}");
//             
//             return dlcUpdateRequest;
//         }
//         
//         private async UniTask<DLCPackageVersion> DownloadPackageVersion(string packageVersionUrl)
//         {
//             Debug.Log($"DownloadPackageVersion: {packageVersionUrl}");
//             using UnityWebRequest request = UnityWebRequest.Get(packageVersionUrl);
//             request.timeout = 10;
//             try
//             {
//                 await request.SendWebRequest();
//                 Debug.Log(request.downloadHandler.text);
//                 
//                 if (request.result == UnityWebRequest.Result.Success) 
//                     return JsonConvert.DeserializeObject<DLCPackageVersion>(request.downloadHandler.text);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogWarning(e);
//             }
//             return null;
//         }
//
//         public void AddPackage(string packageName)
//         {
//             var config = MainLocalConfig.Instance;
//             if (!config.PackageList.Contains(packageName))
//             {
//                 config.PackageList.Add(packageName);
//                 config.Save();
//             }
//         }
//
//         public void RemovePackage(string packageName)
//         {
//             var config = MainLocalConfig.Instance;
//             if (config.PackageList.Contains(packageName))
//             {
//                 config.PackageList.Remove(packageName);
//                 config.Save();
//             }
//         }
//
//         private async UniTask<DLCVersion> DownloadDLCVersion(string url)
//         {
//             Debug.Log($"DownloadDLCVersion: {url}");
//             using UnityWebRequest request = UnityWebRequest.Get(url);
//             request.timeout = 10;
//
//             try
//             {
//                 await request.SendWebRequest();
//                 Debug.Log(request.downloadHandler.text);
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError(ex);
//             }
//
//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 return JsonConvert.DeserializeObject<DLCVersion>(request.downloadHandler.text);
//             }
//             return null;
//         }
//
//         private IEnumerator DLCDownloadRes()
//         {
//             yield return null;
//             Debug.Log($"DLCDownloadRes Length: {_dlcUpdateRequest.DownloadRequests?.Length ?? 0} Size: {FormatHelper.FormatByte(_dlcUpdateRequest.DownloadTotalSize)}");
//             if (_dlcUpdateRequest.DownloadRequests?.Length > 0)
//             {
//                 DownloadTotalBytes = _dlcUpdateRequest.DownloadTotalSize;
//                 foreach (var request in _dlcUpdateRequest.DownloadRequests)
//                 {
//                     _ = Downloader.DownloadAsync(request);
//                 }
//
//                 float len = DownloadTotalBytes;
//                 while (true)
//                 {
//                     DownloadedBytes = _dlcUpdateRequest.DownloadRequests.Sum(item => item.DownloadedBytes);
//                     Progress = DownloadedBytes / len;
//                     // Progress = downloader.DownloadProgress;
//                     // DownloadedBytes = downloader.DownloadedBytes;
//                     // DownloadTotalBytes = downloader.DownloadTotalBytes;
//                     if (!Downloader.IsDownloading) break;
//                     yield return null;
//                 }
//
//                 var errorArr = Downloader.GetErrorRequests();
//                 if (errorArr?.Length > 0)
//                 {
//                     for (int i = 0; i < errorArr.Length; i++)
//                     {
//                         Debug.Log($"Error: ({i + 1}/{errorArr.Length}) {errorArr[i].DownloadUrl}");
//                     }
//                     
//                     F.CoroutineSystem.StartCoroutine(ErrorRetry());
//                     yield break;
//                 }
//             }
//
//             _enterState = ELogicState.DLCUnzipRes;
//         }
//
//         private IEnumerator DLCUnzipRes()
//         {
//             yield return null;
//             Debug.Log($"DLCUnzipRes Length: {_dlcUpdateRequest.UnzipRequests?.Length ?? 0}");
//             if (_dlcUpdateRequest.UnzipRequests?.Length > 0)
//             {
//                 foreach (var request in _dlcUpdateRequest.UnzipRequests) _ = Compressor.UnzipAsync(request);
//                 float len = _dlcUpdateRequest.UnzipRequests.Length;
//                 while (true)
//                 {
//                     Progress = _dlcUpdateRequest.UnzipRequests.Sum(item => item.Progress) / len;
//                     if (!Compressor.IsUnzipping) break;
//                     yield return null;
//                 }
//                 
//                 var errorArr = Compressor.GetErrorRequests();
//                 if (errorArr?.Length > 0)
//                 {
//                     for (int i = 0; i < errorArr.Length; i++)
//                     {
//                         Debug.Log($"Unzip Error: ({i + 1}/{errorArr.Length}) {errorArr[i].ZipFile}");
//                     }
//                     
//                     F.CoroutineSystem.StartCoroutine(ErrorRetry());
//                     yield break;
//                 }
//             }
//             
//             yield return null;
//             
//             FileHelper.ClearDirectory(EasyFrameworkSettings.ISettings.DLCDownloadPath);
//             _enterState = ELogicState.Completed;
//         }
//
//         private IEnumerator ErrorRetry()
//         {
//             _errorRetryCount++;
//             if (_errorRetryCount < EasyFrameworkSettings.Instance.dlcErrorRetryCount)
//             {
//                 yield return new WaitForSeconds(_errorRetryCount);
//                 _enterState = ELogicState.DLCDownloadRes;
//             }
//             else
//             {
//                 _enterState = ELogicState.Error;
//             }
//         }
//     }
// }