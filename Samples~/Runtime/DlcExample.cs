using System;
using EasyFramework;

namespace EasyFramework.Samples.Modules
{
    // 配置 AppSettings 的版本服务、DLC URL、匹配平台清单与专用缓存目录后显式调用。
    // Update 会更新版本和资源；Editor 资源模式成功不代表发布配置已验证。
    public static class DlcExample
    {
        public static async ETask<IDLCManager.EResult> UpdateAsync()
        {
            await F.InitializeAsync();
            var result = await F.DLCManager.UpdateAsync();
            FDebug.Log($"DLC 更新结果：{result}");
            return result;
        }

        public static async ETask<IDLCManager.EResult> UpdateVersionAsync(string version)
        {
            await F.InitializeAsync();
            return await F.DLCManager.UpdateAsync(version);
        }

        // 以下查询和直接下载入口在初始化后调用；Exists 同时校验清单长度与 MD5。
        public static bool Exists(string resourceName) => F.DLCManager.Exists(resourceName);
        public static string GetFileName(string resourceName) => F.DLCManager.GetFileName(resourceName);
        public static string GetFilePath(string resourceName) => F.DLCManager.GetFilePath(resourceName);
        public static ETask<bool> DownloadAsync(string resourceName) => F.DLCManager.DownloadAsync(resourceName);

        // out 路径不是完成通知，必须 await 返回任务并检查 bool 后才读文件。
        public static ETask<bool> DownloadAsync(string resourceName, out string filePath)
            => F.DLCManager.DownloadAsync(resourceName, out filePath);

        public static ETask<string> DownloadAndGetPathAsync(string resourceName)
            => F.DLCManager.DownloadAndReturnFileAsync(resourceName);

        public static async ETask<string> EnsureFileAsync(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("请传入 DLC 清单中的资源名。", nameof(resourceName));

            await F.InitializeAsync();
            if (!F.DLCManager.Exists(resourceName))
            {
                bool downloaded = await F.DLCManager.DownloadAsync(resourceName);
                if (!downloaded) return null;
            }

            // 查询路径本身不保证文件存在；先完成校验或下载。
            return F.DLCManager.Exists(resourceName)
                ? F.DLCManager.GetFilePath(resourceName)
                : null;
        }
    }
}
