// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using EasyFramework.AOT;
// using Newtonsoft.Json;
// using UnityEngine;
//
// namespace EasyFramework.Core
// {
//     public enum EDLCUpdateComponentState
//     {
//         None,
//         Downloading,
//         Unzipping,
//         Completed,
//         Error
//     }
//
//     public class DLCUpdateComponent
//     {
//         public EDLCUpdateComponentState State { get; private set; }
//         public long DownloadedBytes { get; private set; }
//         public long DownloadTotalBytes { get; private set; }
//         public float Progress => GetProgress();
//         public bool IsRunning { get; private set; }
//
//         public DLCVersion DLCVersion { get; private set; }
//         public DLCPackageVersion DLCPackageVersion { get; private set; }
//
//         private List<HttpClientDownloadRequest> _downloadList = new();
//         private List<DLCCompressRequest> _unzipList = new();
//
//         private HttpClientDownloader _httpClientDownloader = new HttpClientDownloader();
//         private CancellationTokenSource _cancellationTokenSource;
//         private int _unzipIndex;
//         private float _unzipProgress;
//         
//         public DLCUpdateComponent(DLCVersion dlcVersion, DLCPackageVersion dlcPackageVersion)
//         {
//             DLCVersion = dlcVersion;
//             DLCPackageVersion = dlcPackageVersion;
//             
//             List<ResFileInfo> listUpdateFiles = new();
//             List<ResFileInfo> zipUpdateFiles = new();
//
//                 var localPackageVersionFile = $"{EasyFrameworkAOTSettings.ISettings.DLCResPath}/{dlcPackageVersion.packageFileName}";
//                 var localPackageVersion = File.Exists(localPackageVersionFile)
//                     ? JsonConvert.DeserializeObject<DLCPackageVersion>(File.ReadAllText(localPackageVersionFile))
//                     : new DLCPackageVersion();
//                 if (dlcPackageVersion.listArray?.Length > 0)
//                 {
//                     var localResListDict = localPackageVersion.listArray?.ToDictionary(item => item.name, item => item);
//                     foreach (var resFileInfo in dlcPackageVersion.listArray)
//                     {
//                         if (localResListDict != null && localResListDict.TryGetValue(resFileInfo.name, out var info) &&
//                             info.md5 == resFileInfo.md5) continue;
//                         listUpdateFiles.Add(resFileInfo);
//                     }
//                 }
//                 if (dlcPackageVersion.zipArray?.Length > 0)
//                 {
//                     var localResZipDict = localPackageVersion.zipArray?.ToDictionary(item => item.name, item => item);
//                     foreach (var resFileInfo in dlcPackageVersion.zipArray)
//                     {
//                         if (localResZipDict != null && localResZipDict.TryGetValue(resFileInfo.name, out var info) &&
//                             info.md5 == resFileInfo.md5) continue;
//                         zipUpdateFiles.Add(resFileInfo);
//                     }
//                 }
//             
//             long listUpdateSize = listUpdateFiles.Sum(item => item.size);
//             long zipUpdateSize = zipUpdateFiles.Sum(item => item.size);
//
//             Debug.Log($"PackageName:{dlcPackageVersion.packageName}  List({listUpdateFiles.Count}, {FormatHelper.FormatByte(listUpdateSize)})  Zip({zipUpdateFiles.Count}, {FormatHelper.FormatByte(zipUpdateSize)})");
//             
//             if (zipUpdateSize < listUpdateSize)
//             {
//                 var serverUrl = $"{EasyFrameworkAOTSettings.ISettings.DLCServerUrl}/{dlcVersion.version}/DLCZip";
//                 
//                 for (int i = 0; i < zipUpdateFiles.Count; i++)
//                 {
//                     var resFileInfo = zipUpdateFiles[i];
//                     string downloadFile = $"{EasyFrameworkAOTSettings.ISettings.DLCDownloadPath}/{resFileInfo.name}";
//
//                     if (File.Exists(downloadFile))
//                     {
//                         if (!string.IsNullOrWhiteSpace(resFileInfo.md5) && MD5Helper.MD5File(downloadFile) == resFileInfo.md5)
//                         {
//                             _unzipList.Add(new DLCCompressRequest(downloadFile, EasyFrameworkAOTSettings.ISettings.DLCResPath));
//                             DownloadedBytes += resFileInfo.size;
//                             continue;
//                         }
//
//                         File.Delete(downloadFile);
//                     }
//                     
//                     string downloadUrl = $"{serverUrl}/{resFileInfo.name}";
//                     var request = new HttpClientDownloadRequest(downloadUrl, downloadFile, resFileInfo.md5, resFileInfo.size);
//                     _downloadList.Add(request);
//                     _unzipList.Add(new DLCCompressRequest(downloadFile, EasyFrameworkAOTSettings.ISettings.DLCResPath));
//                 }
//             }
//             else
//             {
//                 var serverUrl = $"{EasyFrameworkAOTSettings.ISettings.DLCServerUrl}/{dlcVersion.version}/DLCList";
//                 
//                 for (int i = 0; i < listUpdateFiles.Count; i++)
//                 {
//                     var resFileInfo = listUpdateFiles[i];
//                     string downloadFile = $"{EasyFrameworkAOTSettings.ISettings.DLCResPath}/{resFileInfo.name}";
//                     
//                     if (File.Exists(downloadFile))
//                     {
//                         if (!string.IsNullOrWhiteSpace(resFileInfo.md5) && MD5Helper.MD5File(downloadFile) == resFileInfo.md5)
//                         {
//                             DownloadedBytes += resFileInfo.size;
//                             continue;
//                         }
//                         File.Delete(downloadFile);
//                     }
//                     
//                     string downloadUrl = $"{serverUrl}/{resFileInfo.name}";
//                     var request = new HttpClientDownloadRequest(downloadUrl, downloadFile, resFileInfo.md5, resFileInfo.size);
//                     _downloadList.Add(request);
//                 }
//             }
//
//             // Debug.Log($"{FormatHelper.FormatByte(DownloadedBytes)}/{FormatHelper.FormatByte(DownloadedBytes)}");
//             DownloadTotalBytes = _downloadList.Sum(item => item.TotalBytes) + DownloadedBytes;
//         }
//
//         public async Task Start()
//         {
//             if (IsRunning || State == EDLCUpdateComponentState.Completed) return;
//             IsRunning = true;
//
//             _cancellationTokenSource = new CancellationTokenSource();
//             
//             try
//             {
//                 switch (State)
//                 {
//                     case EDLCUpdateComponentState.None:
//                     case EDLCUpdateComponentState.Downloading:
//                         await DownloadAsync();
//                         break;
//                     case EDLCUpdateComponentState.Unzipping:
//                         await UnzipAsync();
//                         break;
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError(e);
//                 State = EDLCUpdateComponentState.Error;
//             }
//             
//             IsRunning = false;
//         }
//
//         public void Pause()
//         {
//             if (State == EDLCUpdateComponentState.None || State == EDLCUpdateComponentState.Completed) return;
//             
//             IsRunning = false;
//             _cancellationTokenSource.Cancel();
//         }
//
//         private float GetProgress()
//         {
//             switch (State)
//             {
//                 case EDLCUpdateComponentState.Unzipping:
//                     float len = _unzipList.Count;
//                     float v = 1 / len;
//                     float p = _unzipIndex / len;
//                     return p + v * _unzipProgress;
//                 case EDLCUpdateComponentState.Downloading:
//                     var cur = DownloadedBytes;
//                     if (_downloadList.Count > 0) cur += _downloadList[^1].DownloadedBytes;
//                     return (float)(cur / (double)DownloadTotalBytes);
//                 case EDLCUpdateComponentState.Completed: return 1;
//             }
//             return 0;
//         }
//
//         private async Task DownloadAsync()
//         {
//             State = EDLCUpdateComponentState.Downloading;
//             if (_downloadList.Count > 0)
//             {
//                 for (int i = _downloadList.Count - 1; i >= 0; i--)
//                 {
//                     var downloadRequest = _downloadList[i];
//                     var result = await _httpClientDownloader.DownloadAsync(downloadRequest, _cancellationTokenSource.Token);
//                     if (!result)
//                     {
//                         State = EDLCUpdateComponentState.Error;
//                         return;
//                     }
//                     if (_cancellationTokenSource.IsCancellationRequested) return;
//                     DownloadedBytes += downloadRequest.TotalBytes;
//                     _downloadList.RemoveAt(i);
//                 }
//             }
//
//             await UnzipAsync();
//         }
//
//         private async Task UnzipAsync()
//         {
//             State = EDLCUpdateComponentState.Unzipping;
//             
//             for (int i = _unzipIndex; i < _unzipList.Count; i++)
//             {
//                 var request = _unzipList[i];
//                 await ZipHelper.UnzipFileAsync(request.ZipFile, EasyFrameworkAOTSettings.ISettings.DLCResPath, _cancellationTokenSource.Token,
//                     (progress) => { _unzipProgress = progress; });
//                 if (_cancellationTokenSource.IsCancellationRequested) return;
//                 _unzipIndex++;
//             }
//             State = EDLCUpdateComponentState.Completed;
//         }
//
//         public static async Task<DLCUpdateComponent> CreateAsync(DLCVersion dlcVersion, DLCPackageVersion packageVersion)
//         {
//             return await UniTask.RunOnThreadPool(() => new DLCUpdateComponent(dlcVersion, packageVersion));
//         }
//         
//     }
// }