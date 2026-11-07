/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        
        public string DLCVersionInfoLocalFile => Path.Combine(_dlcPath, DLCVersionInfo.FileName);
        public DLCVersionInfo VersionInfo { get; private set; }

        protected virtual string VersionUid => DLCConfig.Instance.DLCVersionInfoUid;
        protected virtual string ServerUrl => DLCConfig.Instance.DLCServerUrl;

        private readonly string _dlcPath;
        private readonly SemaphoreSlim _downloadSlots;
        private readonly Dictionary<string, Task<string>> _downloads = new();
        private string _currentServerUrl;

        public DLCUpdater() : this(EasyFrameworkSettings.Instance.DLCPath, EasyFrameworkSettings.Instance.downloadParallel) { }

        internal DLCUpdater(string dlcPath, int parallel = 3)
        {
            _dlcPath = Path.GetFullPath(dlcPath);
            _downloadSlots = new SemaphoreSlim(Math.Max(1, parallel));
        }

        public async ETask<EResult> UpdateAsync()
        {
            if (!await InitializeAsync()) return EResult.VersionError;

            var versionInfo = VersionInfo;
            var serverUrl = _currentServerUrl;
            var tasks = new List<Task<string>>();
            foreach (var info in versionInfo.hashFiles) tasks.Add(GetOrDownloadAsync(info, serverUrl));
            var files = await Task.WhenAll(tasks);
            if (VersionInfo != versionInfo) return EResult.VersionError;
            return files.All(file => !string.IsNullOrEmpty(file)) ? EResult.Success : EResult.DownloadError;
        }

        public string GetResServerUrl(string resName)
        {
            var info = GetFileInfo(resName);
            if (info == null) return string.Empty;
            return $"{_currentServerUrl}/{info.fileName}";
        }

        public string GetFileName(string resName) => GetFileInfo(resName)?.fileName;
        public string GetFilePath(string resName)
        {
            var info = GetFileInfo(resName);
            return info == null ? string.Empty : Path.Combine(_dlcPath, info.fileName);
        }

        public bool Exists(string resName)
        {
            var info = GetFileInfo(resName);
            return info != null && IsValidFile(Path.Combine(_dlcPath, info.fileName), info);
        }

        public async ETask<bool> DownloadAsync(string resName)
        {
            var filePath = await DownloadAndReturnFileAsync(resName);
            return !filePath.IsNullOrWhiteSpace();
        }

        public async ETask<string> DownloadAndReturnFileAsync(string resName)
        {
            var versionInfo = VersionInfo;
            var info = GetFileInfo(resName);
            if (info == null) return string.Empty;
            var file = await GetOrDownloadAsync(info, _currentServerUrl);
            return VersionInfo == versionInfo ? file : string.Empty;
        }

        public async ETask<bool> InitializeAsync(string url = null)
        {
            try
            {
                var uid = VersionUid;
                var serverUrl = ServerUrl;
                bool explicitUrl = !url.IsNullOrWhiteSpace();
                if (!explicitUrl && IsValidVersion(VersionInfo, uid))
                {
                    _currentServerUrl = serverUrl;
                    return true;
                }

                DLCVersionInfo versionInfo = null;
                if (!explicitUrl)
                {
                    try
                    {
                        if (File.Exists(DLCVersionInfoLocalFile)) versionInfo = LoadVersionInfo(File.ReadAllText(DLCVersionInfoLocalFile));
                    }
                    catch (Exception e) { FDebug.LogException(e); }
                }
                if (!IsValidVersion(versionInfo, explicitUrl ? null : uid))
                {
                    url = explicitUrl ? url : $"{serverUrl}/{DLCVersionInfo.FileName}";
                    var content = await LoadTextAsync(url);
                    versionInfo = LoadVersionInfo(content);
                    if (!IsValidVersion(versionInfo, explicitUrl ? null : uid)) return false;
                }
                if (uid != VersionUid || serverUrl != ServerUrl) return false;

                versionInfo.RefreshNames();
                var tempFile = FileHelper.CreateDownloadTempPath(DLCVersionInfoLocalFile);
                try
                {
                    ConfigHelper.Save(versionInfo, tempFile);
                    FileHelper.ReplaceFileFromTemp(tempFile, DLCVersionInfoLocalFile);
                }
                finally { FileHelper.DeleteFile(tempFile); }

                VersionInfo = versionInfo;
                _currentServerUrl = serverUrl;
                DeleteUnversionedFiles();
                return true;
            }
            catch (Exception e) { FDebug.LogException(e); }
            return false;
        }
        
        public void DeleteUnversionedFiles()
        {
            if (!IsValidVersion(VersionInfo, null) || !Directory.Exists(_dlcPath)) return;
            try
            {
                if ((File.GetAttributes(_dlcPath) & FileAttributes.ReparsePoint) != 0) return;
                var filesToKeep = VersionInfo.hashFiles.Select(info => info.fileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var file in Directory.GetFiles(_dlcPath))
                {
                    var fileName = Path.GetFileName(file);
                    if (filesToKeep.Contains(fileName) || FileHelper.ExtractMD5FromFileName(fileName) == null) continue;
                    if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0) continue;
                    lock (_downloads) if (_downloads.ContainsKey(file)) continue;
                    File.Delete(file);
                    FDebug.Log($"Delete: {file}");
                }
            }
            catch (Exception e) { FDebug.LogException(e); }
        }

        private HashFileInfo GetFileInfo(string resName) =>
            string.IsNullOrEmpty(resName) ? null : VersionInfo?.GetFileInfo(resName);

        private static DLCVersionInfo LoadVersionInfo(string content) =>
            ConfigHelper.LoadFromText<DLCVersionInfo>(content, EConfigType.Newtonsoft, EConfigEncrypt.None, null);

        private static bool IsValidVersion(DLCVersionInfo versionInfo, string uid)
        {
            if (versionInfo == null || string.IsNullOrWhiteSpace(versionInfo.uid) || versionInfo.hashFiles == null) return false;
            if (!string.IsNullOrEmpty(uid) && versionInfo.uid != uid) return false;
            var names = new HashSet<string>();
            var lengths = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var info in versionInfo.hashFiles)
            {
                if (info == null || string.IsNullOrWhiteSpace(info.resName) || !names.Add(info.resName) || info.length < 0) return false;
                if (string.IsNullOrEmpty(info.fileName) || info.fileName.IndexOfAny(new[] { '/', '\\', ':' }) >= 0) return false;
                if (info.fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || FileHelper.ExtractMD5FromFileName(info.fileName) == null) return false;
                if (lengths.TryGetValue(info.fileName, out var length) && length != info.length) return false;
                lengths[info.fileName] = info.length;
            }
            return true;
        }

        private static bool IsValidFile(string file, HashFileInfo info)
        {
            try { return FileHelper.VerifyFileIntegrity(file, info.length, FileHelper.ExtractMD5FromFileName(info.fileName)); }
            catch (Exception e) { FDebug.LogException(e); return false; }
        }

        private Task<string> GetOrDownloadAsync(HashFileInfo info, string serverUrl)
        {
            var file = Path.Combine(_dlcPath, info.fileName);
            if (IsValidFile(file, info)) return Task.FromResult(file);
            TaskCompletionSource<string> completion;
            lock (_downloads)
            {
                if (_downloads.TryGetValue(file, out var task)) return task;
                completion = new TaskCompletionSource<string>();
                _downloads.Add(file, completion.Task);
            }
            _ = DownloadAsync(info, serverUrl, file, completion);
            return completion.Task;
        }

        private async Task DownloadAsync(HashFileInfo info, string serverUrl, string file, TaskCompletionSource<string> completion)
        {
            var result = string.Empty;
            string tempFile = null;
            await _downloadSlots.WaitAsync();
            try
            {
                if (IsValidFile(file, info)) result = file;
                else
                {
                    tempFile = FileHelper.CreateDownloadTempPath(file);
                    bool success = await DownloadFileAsync($"{serverUrl}/{info.fileName}", tempFile);
                    if (success && IsValidFile(tempFile, info))
                    {
                        FileHelper.ReplaceFileFromTemp(tempFile, file);
                        result = file;
                    }
                }
            }
            catch (Exception e) { FDebug.LogException(e); }
            finally
            {
                FileHelper.DeleteFile(tempFile);
                _downloadSlots.Release();
                lock (_downloads) _downloads.Remove(file);
                completion.TrySetResult(result);
            }
        }

        protected virtual async ETask<string> LoadTextAsync(string url)
        {
            var request = await ETask.UnityWebRequest(url);
            return request != null && request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null;
        }

        protected virtual ETask<bool> DownloadFileAsync(string url, string file) => F.HttpManager.DownloadAsync(url, file);
    }
}
