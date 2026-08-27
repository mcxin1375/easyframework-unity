/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

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
        
        public DLCVersionInfo VersionInfo { get; private set; }
        private DownloadTask _downloadTask;

        public string DLCServerUrl { get; private set; }
        public string DLCVersionInfoLocalFile => $"{EasyFrameworkSettings.Instance.DLCPath}/{DLCVersionInfo.FileName}";

        public async ETask<EResult> UpdateAsync()
        {
            await InitAsync();
            if (VersionInfo == null) return EResult.VersionInfoError;

            _downloadTask = new();
            foreach (var info in VersionInfo.hashFiles)
            {
                // FDebug.Log($"{info.resName}, {info.fileName}");
                var downloadFile = $"{EasyFrameworkSettings.Instance.DLCPath}/{info.fileName}";
                if (File.Exists(downloadFile)) continue;

                var downloadUrl = $"{DLCServerUrl}/{info.fileName}";
                _downloadTask.AddRequest(downloadUrl, downloadFile, info.length);
            }

            FDebug.Log("Download Task Start");
            var result = await _downloadTask.StartAsync();
            FDebug.Log("Download Task End: " + result);
            
            if (!result) return EResult.DownloadError;
            
            return EResult.Success;
        }

        public async ETask InitAsync()
        {
            var versionName = EasyFrameworkConfig.Instance.dlcVersion;
            DLCServerUrl = DLCHelper.GetDLCServerURL(versionName);
            
            var currentVersionInfo = ConfigHelper.Load<DLCVersionInfo>(DLCVersionInfoLocalFile);
            if (currentVersionInfo != null && currentVersionInfo.uid == EasyFrameworkConfig.Instance.dlcVersionInfoUid)
            {
                VersionInfo = currentVersionInfo;
                return;
            }
            
            var url = $"{DLCServerUrl}/{DLCVersionInfo.FileName}";
            var unityWebRequest = await ETask.UnityWebRequest(url);
            if (unityWebRequest.result != UnityWebRequest.Result.Success) return;
            
            File.WriteAllText(DLCVersionInfoLocalFile, unityWebRequest.downloadHandler.text);
            VersionInfo = ConfigHelper.LoadFromText<DLCVersionInfo>(unityWebRequest.downloadHandler.text);

            EasyFrameworkConfig.Instance.dlcVersionInfoUid = VersionInfo.uid;
            EasyFrameworkConfig.Instance.Save();

            DeleteUnversionedFiles();
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