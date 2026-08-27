/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    [Serializable]
    public class StreamingAssetsResZipInfo
    {
        public string mainResUid;
        public ResFileInfo[] mainResZipArray;
    }

    public enum EResLoaderMode
    {
        /// <summary>
        /// StreamingAssets
        /// </summary>
        DLC_StreamingAssets,
        /// <summary>
        /// Cdn
        /// </summary>
        DLC_CDN
    }

    [ProjectSettings(ProjectSettingsAttribute.ETag.Resources)]
    public class EasyFrameworkSettings : ProjectSettings<EasyFrameworkSettings>
    {
        [Header("Framework Settings")] 
        public bool autoInitialize = true;
        
        [Header("App Settings")]
        public AppSettings appSettings;
        
        [Header("AssetBundle Settings")]
        public string abSuffix = ".ab";
        
        [Header("ResLoader Settings")]
        public bool resLoaderEditorMode = true;
        public EResLoaderMode resLoaderMode = EResLoaderMode.DLC_StreamingAssets;
        public int resRequestAliveTime = 60;
        
        /// <summary>
        /// 版本索引，发布时底包会记录该值，判断一致才可热更新
        /// </summary>
        [Header("DLCManager")]
        public int dlcVersionIndex = 1;
        
        [Header("HttpManager")]
        public int downloadParallel = 3;
        public int unzipParallel = 3;
        public int maxRetryCount = 3;
        public int retryDelayMs = 1000;
        
        [Header("HttpManager")] 
        public GameObject uiRoot;
        public Vector2 resolution = new Vector2(1920, 1080);
        public EUIRenderMode uiRenderMode = EUIRenderMode.UICamera;
        
        [Header("HybridCLRManager")]
        public string enterType = "HotUpdate";
        public string enterMethod = "Enter";

        [Header("发布时动态更新")]
        public StreamingAssetsResZipInfo streamingAssetsResZipInfo = new();
        
        [Header("Debug Settings")]
        public EDebugLevel debugLevel = EDebugLevel.Log | EDebugLevel.LogWarning | EDebugLevel.LogError;

        public string DataPath { get; private set; }
        public string DLCPath { get; private set; }
        public string ConfigPath { get; private set; }
        public string DownloadPath { get; private set; }

        private static IAppSettings _appSettings;
        public static IAppSettings AppSettings
        {
            get
            {
                if (Instance.appSettings != null) return Instance.appSettings;
                _appSettings ??= EasyFrameworkReflection.CreateInstance<IAppSettings>();
                return _appSettings;
            }
        }

        private static IApp _app;
        public static IApp App
        {
            get
            {
                if (_app == null)
                {
                    // var type = System.Type.GetType(Instance.appFullType);
                    // if (type != null)
                    // {
                    //     _app = Activator.CreateInstance(type) as IApp;
                    // }
                    // else
                    // {
                    //     _app = ReflectionHelper.CreateInstance<IApp>();
                    // }
                    _app = EasyFrameworkReflection.CreateInstance<IApp>();
                }
                return _app;
            }
        }

        protected override void OnCreate()
        {
#if UNITY_EDITOR
            DataPath = Application.persistentDataPath;
#elif UNITY_IOS
            DataPath = Application.temporaryCachePath;
#else
            DataPath = Application.persistentDataPath;
#endif
            DLCPath = $"{DataPath}/DLC";
            ConfigPath = $"{DataPath}/Config";
            DownloadPath = $"{DataPath}/Download";

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                FileHelper.CreateDirectory(DLCPath);
                FileHelper.CreateDirectory(ConfigPath);
                FileHelper.CreateDirectory(DownloadPath);
            }
#else
            FileHelper.CreateDirectory(DLCPath);
            FileHelper.CreateDirectory(ConfigPath);
            FileHelper.CreateDirectory(DownloadPath);
#endif
        }
    }
}