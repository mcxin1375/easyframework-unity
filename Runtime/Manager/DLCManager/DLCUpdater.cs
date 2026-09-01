/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

namespace EasyFramework
{
    internal class DLCUpdater : Singleton<DLCUpdater>
    {
        public enum EResult
        {
            Success,
            VersionError,
            DownloadError,
        }
        
        public string DLCVersionInfoLocalFile => $"{EasyFrameworkSettings.Instance.DLCPath}/{DLCVersionInfo.FileName}";
        public DLCVersionInfo VersionInfo { get; private set; }
        
        private DLCDownloader _dlcDownloader;
        private readonly Dictionary<string, string> _fileDict = new();

        public async ETask<EResult> UpdateAsync()
        {
            await InitializeAsync();
            if (VersionInfo == null) return EResult.VersionError;

            _dlcDownloader = new();
            foreach (var info in VersionInfo.hashFiles)
            {
                // FDebug.Log($"{info.resName}, {info.fileName}");
                var downloadFile = $"{EasyFrameworkSettings.Instance.DLCPath}/{info.fileName}";
                if (File.Exists(downloadFile)) continue;

                var downloadUrl = $"{EasyFrameworkConfig.Instance.DLCServerUrl}/{info.fileName}";
                _dlcDownloader.AddRequest(downloadUrl, downloadFile, info.length);
            }

            FDebug.Log("Download Task Start");
            var result = await _dlcDownloader.StartAsync();
            FDebug.Log("Download Task End: " + result);
            
            if (!result) return EResult.DownloadError;
            
            return EResult.Success;
        }

        public string GetResServerUrl(string resName)
        {
            var info = VersionInfo?.GetFileInfo(resName);
            if (info == null) return string.Empty;
            return $"{EasyFrameworkConfig.Instance.DLCServerUrl}/{info.fileName}";
        }

        public string GetFileName(string resName) => VersionInfo?.GetFileName(resName);
        public string GetFilePath(string resName)
        {
            if (_fileDict.TryGetValue(resName, out var filePath)) return filePath;
            var info = VersionInfo?.GetFileInfo(resName);
            if (info == null)
            {
                FDebug.LogError($"[DLCUpdater] Res not found: {resName}");
                return string.Empty;
            }

            filePath = $"{EasyFrameworkSettings.Instance.DLCPath}/{info.fileName}";
            _fileDict.Add(resName, filePath);
            return filePath;
        }

        public bool Exists(string resName)
        {
            var filePath = GetFilePath(resName);
            return string.IsNullOrWhiteSpace(filePath) || File.Exists(filePath);
        }

        public async ETask<bool> DownloadAsync(string resName)
        {
            var filePath = await DownloadAndReturnFileAsync(resName);
            return !filePath.IsNullOrWhiteSpace();
        }

        public async ETask<string> DownloadAndReturnFileAsync(string resName)
        {
            var filePath = GetFilePath(resName);
            if (File.Exists(filePath)) return filePath;

            var downloadUrl = GetResServerUrl(resName);
            FDebug.LogWarning($"[Download] {resName} - {downloadUrl}");
            var result = await F.HttpManager.DownloadAsync(downloadUrl, filePath);
            if (result)
            {
                FDebug.LogWarning($"[Download] {resName} - true");
                return filePath;
            }
            else
            {
                FDebug.LogWarning($"[Download] {resName} - false");
                return string.Empty;
            }
        }

        public async ETask InitializeAsync(string url = null)
        {
            try
            {
                if (url.IsNullOrWhiteSpace())
                {
                    url = $"{EasyFrameworkConfig.Instance.DLCServerUrl}/{DLCVersionInfo.FileName}";
                }
                
                if (VersionInfo != null && VersionInfo.uid == EasyFrameworkConfig.Instance.DLCVersionInfoUid)
                    return;
                
                VersionInfo = ConfigHelper.Load<DLCVersionInfo>(DLCVersionInfoLocalFile);
                if (VersionInfo == null || VersionInfo.uid != EasyFrameworkConfig.Instance.DLCVersionInfoUid)
                {
                    // FDebug.Log($"Request: {url}");
                    var unityWebRequest = await ETask.UnityWebRequest(url);
                    if (unityWebRequest.result != UnityWebRequest.Result.Success)
                    {
                        FDebug.LogError($"[UWR Error] {url}");
                        return;
                    }
                    FDebug.Log($"Request: {url}\n{unityWebRequest.downloadHandler.text}");

                    File.WriteAllText(DLCVersionInfoLocalFile, unityWebRequest.downloadHandler.text);
                    VersionInfo = ConfigHelper.LoadFromText<DLCVersionInfo>(unityWebRequest.downloadHandler.text);
                }
            
                VersionInfo?.RefreshNames();
                DeleteUnversionedFiles();
                
                FDebug.Log($"DLCUpdater Initialized! Count: {VersionInfo?.hashFiles.Length ?? -1}");
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
        }
        
        public void DeleteUnversionedFiles()
        {
            if (VersionInfo == null) return;
            
            var hashSet = VersionInfo.hashFiles.Select(item => item.fileName).ToHashSet();
            var files = Directory.GetFiles(EasyFrameworkSettings.Instance.DataPath);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (!hashSet.Contains(fileName))
                {
                    File.Delete(file);
                    FDebug.Log($"Delete: {file}");
                }
            }
        }
    }
}