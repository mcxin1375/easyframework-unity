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
    public class DLCUpdater : Singleton<DLCUpdater>
    {
        public enum EResult
        {
            Success,
            VersionInfoError,
            DownloadError,
        }
        
        public string DLCVersionInfoLocalFile => $"{EasyFrameworkSettings.Instance.DLCPath}/{DLCVersionInfo.FileName}";
        public DLCVersionInfo VersionInfo { get; private set; }
        
        private DownloadTask _downloadTask;
        private readonly Dictionary<string, HashFileInfo> _infoDict = new();
        private readonly Dictionary<string, string> _nameDict = new();
        private readonly Dictionary<string, string> _fileDict = new();


        public async ETask<EResult> UpdateAsync()
        {
            await InitializeAsync();
            if (VersionInfo == null) return EResult.VersionInfoError;

            _downloadTask = new();
            foreach (var info in VersionInfo.hashFiles)
            {
                // FDebug.Log($"{info.resName}, {info.fileName}");
                var downloadFile = $"{EasyFrameworkSettings.Instance.DLCPath}/{info.fileName}";
                if (File.Exists(downloadFile)) continue;

                var downloadUrl = $"{EasyFrameworkConfig.Instance.DLCServerUrl}/{info.fileName}";
                _downloadTask.AddRequest(downloadUrl, downloadFile, info.length);
            }

            FDebug.Log("Download Task Start");
            var result = await _downloadTask.StartAsync();
            FDebug.Log("Download Task End: " + result);
            
            if (!result) return EResult.DownloadError;
            
            return EResult.Success;
        }

        public string GetResFilePath(string resName)
        {
            if (_fileDict.TryGetValue(resName, out var filePath)) return filePath;
            if (!_infoDict.TryGetValue(resName, out var info))
            {
                FDebug.LogError($"DLC Res Not Found: {resName}");
                return string.Empty;
            }

            filePath = $"{EasyFrameworkSettings.Instance.DLCPath}/{info.fileName}";
            _fileDict.Add(resName, filePath);
            return filePath;
        }

        public string GetResServerUrl(string resName)
        {
            if (!_infoDict.TryGetValue(resName, out var info)) return string.Empty;
            return $"{EasyFrameworkConfig.Instance.DLCServerUrl}/{info.fileName}";
        }

        public async ETask<bool> DownloadAsync(string resName)
        {
            var filePath = await DownloadAndReturnFileAsync(resName);
            return !filePath.IsNullOrWhiteSpace();
        }

        public async ETask<string> DownloadAndReturnFileAsync(string resName)
        {
            var filePath = GetResFilePath(resName);
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

        public async ETask InitializeAsync()
        {
            try
            {
                VersionInfo = ConfigHelper.Load<DLCVersionInfo>(DLCVersionInfoLocalFile);
                if (VersionInfo == null || VersionInfo.uid != EasyFrameworkConfig.Instance.DLCVersionInfoUid)
                {
                    var url = $"{EasyFrameworkConfig.Instance.DLCServerUrl}/{DLCVersionInfo.FileName}";
                    // FDebug.Log($"Request: {url}");
                    var unityWebRequest = await ETask.UnityWebRequest(url);
                    if (unityWebRequest.result != UnityWebRequest.Result.Success)
                    {
                        FDebug.LogError($"[Download Error]\n{url}");
                        return;
                    }
                    // FDebug.Log($"Request: {unityWebRequest.downloadHandler.text}");

                    File.WriteAllText(DLCVersionInfoLocalFile, unityWebRequest.downloadHandler.text);
                    VersionInfo = ConfigHelper.LoadFromText<DLCVersionInfo>(unityWebRequest.downloadHandler.text);
                }
            
                _infoDict.Clear();
                _nameDict.Clear();

                if (VersionInfo != null)
                {
                    foreach (var info in VersionInfo.hashFiles)
                    {
                        _infoDict[info.resName] = info;
                        _nameDict[info.resName] = info.fileName;
                    }
                    DeleteUnversionedFiles();
                }
                
                FDebug.Log($"DLCUpdater Initialized! Count: {VersionInfo?.hashFiles.Length ?? -1}");
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
        }
        
        private void DeleteUnversionedFiles()
        {
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
        
        // private sealed class DeleteUnversionedTask : ETask.TaskAwaiter<UnityWebRequest>
        // {
        //     protected override bool OnTaskTick()
        //     {
        //         return true;
        //     }
        //     protected override void OnTaskStart()
        //     {
        //         
        //     }
        //     protected override void OnTaskResult(ETaskStatus status)
        //     {
        //
        //     }
        //     public static ETask StartAsync()
        //     {
        //         var task = new DeleteUnversionedTask();
        //         task.Start(out var token);
        //         return new ETask(task, token);
        //     }
        // }
    }
}