using UnityEngine;

namespace EasyFramework.Samples.Modules
{
    public static class InitializationExample
    {
        // 配置好框架和资源模式后，在 Unity 主线程 await RunAsync()，再使用资源、窗口等模块。
        public static async ETask RunAsync()
        {
            await F.InitializeAsync();
            Debug.Log($"框架已初始化，资源缓存时间：{F.Settings.resRequestAliveTime} 秒");
        }

        public static void LogSettings()
        {
            var settings = F.Settings;
            Debug.Log($"Auto={settings.autoInitialize}, App={settings.appSettings}, AB={settings.abSuffix}, " +
                $"EditorResources={settings.resLoaderEditorMode}, ResourceMode={settings.resLoaderMode}");
            Debug.Log($"Data={settings.DataPath}, Config={settings.ConfigPath}, DLC={settings.DLCPath}, " +
                $"Download={settings.DownloadPath}, Temp={settings.DownloadTempPath}, StreamingAssets={settings.StreamingAssetsDLCPath}");
            Debug.Log($"DLC index={settings.dlcVersionIndex}, RelativePath={settings.streamingAssetsResRelativePath}, " +
                $"Retry={settings.maxRetryCount}/{settings.retryDelayMs}ms, Cache={settings.resRequestAliveTime}s");
            Debug.Log($"UIRoot={settings.uiRoot}, UI={settings.resolution}/{settings.uiRenderMode}, " +
                $"Downloads={settings.downloadParallel}, Unzip={settings.unzipParallel}, " +
                $"Entry={settings.enterType}.{settings.enterMethod}, Debug={settings.debugLevel}");
        }
    }
}
