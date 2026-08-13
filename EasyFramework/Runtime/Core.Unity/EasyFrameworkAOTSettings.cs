/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
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

    public class EasyFrameworkAOTSettings : ProjectSettingsResources<EasyFrameworkAOTSettings>
    {
        [Header("Framework")] 
        public bool autoInitialize = true;
        public EDebugLevel debugLevel = EDebugLevel.Log | EDebugLevel.LogWarning | EDebugLevel.LogError;
        
        [Header("HttpManager")]
        public int maxRetryCount = 3;
        public int retryDelayMs = 1000;
        public int downloadParallel = 3;
        public int unzipParallel = 3;
        
        [Header("WindowManager")] 
        public GameObject uiRoot;
        public Vector2 resolution = new Vector2(1920, 1080);
        public EUIRenderMode uiRenderMode = EUIRenderMode.UICamera;
        
        [Header("HybridCLRManager")]
        public string enterType = "HotUpdate";
        public string enterMethod = "Enter";

        [Header("发布时动态更新")]
        public StreamingAssetsResZipInfo streamingAssetsResZipInfo = new();

        
        
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

        // public EHttpManagerType httpManagerType = EHttpManagerType.UnityWebRequest;
        // public enum EHttpManagerType
        // {
        //     UnityWebRequest,
        //     NetHttpClient
        // }

    }
}