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
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using EasyFramework.DLC;
// using EasyFramework.Internal;
// using Newtonsoft.Json;
// using UnityEngine;
//
// namespace EasyFramework.Core
// {
//     public class DLCUpdateRequest
//     {
//         public long DownloadTotalSize { get; }
//         public DLCVersion DLCVersion { get; }
//         public DLCPackageVersion[] PackageVersions { get; }
//         public DLCDownloaderRequest[] DownloadRequests { get; }
//         public DLCCompressRequest[] UnzipRequests { get; }
//
//         public DLCUpdateRequest(DLCVersion dlcVersion, DLCPackageVersion[] packageVersions, DLCDownloaderRequest[] downloads)
//         {
//             DLCVersion = dlcVersion;
//             PackageVersions = packageVersions;
//             DownloadRequests = downloads;
//             UnzipRequests = new DLCCompressRequest[0];
//             DownloadTotalSize = downloads.Sum(item => item.TotalBytes);
//         }
//         
//         public DLCUpdateRequest(DLCVersion dlcVersion, DLCPackageVersion[] packageVersions, DLCDownloaderRequest[] downloads, DLCCompressRequest[] unzips)
//         {
//             PackageVersions = packageVersions;
//             DownloadRequests = downloads;
//             UnzipRequests = unzips;
//             DownloadTotalSize = downloads.Sum(item => item.TotalBytes);
//         }
//
//         public static DLCUpdateRequest Create(string serverUrl, DLCVersion dlcVersion, DLCPackageVersion packageVersion, Action<float> progressAction = null) =>
//             Create(serverUrl, dlcVersion, new DLCPackageVersion[] { packageVersion }, progressAction);
//         public static DLCUpdateRequest Create(string serverUrl, DLCVersion dlcVersion, DLCPackageVersion[] packageVersionArr, Action<float> progressAction = null)
//         {
//             List<ResFileInfo> listUpdateFiles = new();
//             List<ResFileInfo> zipUpdateFiles = new();
//
//             foreach (var packageVersion in packageVersionArr)
//             {
//                 var localPackageVersionFile = $"{EasyFrameworkSettings.ISettings.DLCResPath}/{packageVersion.packageFileName}";
//                 var localPackageVersion = File.Exists(localPackageVersionFile)
//                     ? JsonConvert.DeserializeObject<DLCPackageVersion>(File.ReadAllText(localPackageVersionFile))
//                     : new DLCPackageVersion();
//                 if (packageVersion.listArray?.Length > 0)
//                 {
//                     var localResListDict = localPackageVersion.listArray?.ToDictionary(item => item.name, item => item);
//                     foreach (var resFileInfo in packageVersion.listArray)
//                     {
//                         if (localResListDict != null && localResListDict.TryGetValue(resFileInfo.name, out var info) &&
//                             info.md5 == resFileInfo.md5) continue;
//                         listUpdateFiles.Add(resFileInfo);
//                     }
//                 }
//                 if (packageVersion.zipArray?.Length > 0)
//                 {
//                     var localResZipDict = localPackageVersion.zipArray?.ToDictionary(item => item.name, item => item);
//                     foreach (var resFileInfo in packageVersion.zipArray)
//                     {
//                         if (localResZipDict != null && localResZipDict.TryGetValue(resFileInfo.name, out var info) &&
//                             info.md5 == resFileInfo.md5) continue;
//                         zipUpdateFiles.Add(resFileInfo);
//                     }
//                 }
//             }
//             
//             long listUpdateSize = listUpdateFiles.Sum(item => item.size);
//             long zipUpdateSize = zipUpdateFiles.Sum(item => item.size);
//
//             Debug.Log($"DLCList - Count: {listUpdateFiles.Count}, Size: {FormatHelper.FormatByte(listUpdateSize)}");
//             Debug.Log($"DLCZip - Count: {zipUpdateFiles.Count}, Size: {FormatHelper.FormatByte(zipUpdateSize)}");
//             
//             if (zipUpdateSize < listUpdateSize)
//             {
//                 var dlcZipServerUrl = $"{serverUrl}/{dlcVersion.version}/DLCZip";
//                 
//                 List<DLCDownloaderRequest> downloadList = new();
//                 List<DLCCompressRequest> unzipList = new();
//                 for (int i = 0; i < zipUpdateFiles.Count; i++)
//                 {
//                     var resFileInfo = zipUpdateFiles[i];
//                     string downloadFile = $"{EasyFrameworkSettings.ISettings.DLCDownloadPath}/{resFileInfo.name}";
//
//                     if (File.Exists(downloadFile))
//                     {
//                         if (!string.IsNullOrWhiteSpace(resFileInfo.md5) && MD5Helper.MD5File(downloadFile) == resFileInfo.md5)
//                         {
//                             unzipList.Add(new DLCCompressRequest(downloadFile, EasyFrameworkSettings.ISettings.DLCResPath));
//                             continue;
//                         }
//
//                         File.Delete(downloadFile);
//                     }
//                     
//                     string downloadUrl = $"{dlcZipServerUrl}/{resFileInfo.name}";
//                     var request = new DLCDownloaderRequest(downloadUrl, downloadFile, resFileInfo.md5, resFileInfo.size);
//                     downloadList.Add(request);
//                     unzipList.Add(new DLCCompressRequest(downloadFile, EasyFrameworkSettings.ISettings.DLCResPath));
//
//                     progressAction?.Invoke((i + 1) / (float)zipUpdateFiles.Count);
//                 }
//
//                 return new DLCUpdateRequest(dlcVersion, packageVersionArr, downloadList.ToArray(), unzipList.ToArray());
//             }
//             else
//             {
//                 var dlcListServerUrl = $"{serverUrl}/{dlcVersion.version}/DLCList";
//                 
//                 List<DLCDownloaderRequest> downloadList = new();
//                 for (int i = 0; i < listUpdateFiles.Count; i++)
//                 {
//                     var resFileInfo = listUpdateFiles[i];
//                     string downloadFile = $"{EasyFrameworkSettings.ISettings.DLCResPath}/{resFileInfo.name}";
//                     
//                     if (File.Exists(downloadFile))
//                     {
//                         if (!string.IsNullOrWhiteSpace(resFileInfo.md5) && MD5Helper.MD5File(downloadFile) == resFileInfo.md5)
//                         {
//                             continue;
//                         }
//                         File.Delete(downloadFile);
//                     }
//                     
//                     string downloadUrl = $"{dlcListServerUrl}/{resFileInfo.name}";
//                     var request = new DLCDownloaderRequest(downloadUrl, downloadFile, resFileInfo.md5, resFileInfo.size);
//                     downloadList.Add(request);
//                     
//                     progressAction?.Invoke((i + 1) / (float)listUpdateFiles.Count);
//                 }
//                 return new DLCUpdateRequest(dlcVersion, packageVersionArr, downloadList.ToArray());
//             }
//         }
//
//         public static async Task<DLCUpdateRequest> CreateAsync(string serverUrl, DLCVersion dlcVersion, DLCPackageVersion packageVersion, Action<float> progressAction = null)
//         {
//             return await UniTask.RunOnThreadPool(() => Create(serverUrl, dlcVersion, packageVersion, progressAction));
//         }
//         public static async Task<DLCUpdateRequest> CreateAsync(string serverUrl, DLCVersion dlcVersion, DLCPackageVersion[] packageVersions, Action<float> progressAction = null)
//         {
//             return await UniTask.RunOnThreadPool(() => Create(serverUrl, dlcVersion, packageVersions, progressAction));
//         }
//
//     }
// }