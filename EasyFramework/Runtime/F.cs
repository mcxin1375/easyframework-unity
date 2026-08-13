/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    
    public static class FAOT
    {
        public static EasyFrameworkBehaviour Behaviour { get; private set; }
        public static IHttpManager HttpManager { get; private set; }

        public static LocalStorageManager LocalStorageManager => LocalStorageManager.Instance;
        
        public static IWindowManager WindowManager => EasyFramework.WindowManager.Instance;
        
        public static MainResManager MainResManager => MainResManager.Instance;
        public static DLCManager DLCManager => DLCManager.Instance;
#if EASYFRAMEWORK_HYBRIDCLR
        public static HybridCLRManager HybridCLRManager => HybridCLRManager.Instance;
#endif
        
        private static bool _initialized;
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            FDebug.Debugger = UnityDebugger.Instance;
            FDebug.DebugLevel = EasyFrameworkAOTSettings.Instance.debugLevel;
            Behaviour = EasyFrameworkBehaviour.Instance;
            HttpManager = UnityWebRequestManager.Instance;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            var settings = EasyFrameworkAOTSettings.CreateInstance();
            if (!settings.autoInitialize) return;
            Initialize();
        }
    }
    
    public static class F
    {
        public static readonly FWorld World = new();
        
        public static EasyFrameworkSettings Settings => EasyFrameworkSettings.Instance;
        public static FBehaviour Behaviour { get; private set; }
        
        public static Event Event => Event.Instance;

        public static IResLoader ResLoader
        {
            get
            {
#if UNITY_EDITOR
                return EditorBridge.Instance.ResLoader;
#endif
                return AssetBundleLoader.Instance;
            }
        }
        
        public static DLCDownloader DLCDownloader => EasyFramework.DLCDownloader.Instance;
        public static IWindowManager WindowManager => EasyFramework.WindowManager.Instance;
        public static ISpriteLoader SpriteLoader => EasyFramework.SpriteLoader.Instance;
        public static IShaderLoader ShaderLoader => EasyFramework.ShaderLoader.Instance;
        public static ISceneLoader SceneLoader => EasyFramework.SceneLoader.Instance;
        public static IControllerManager ControllerManager => EasyFramework.ControllerManager.Instance;
        public static IInputManager InputManager => EasyFramework.InputManager.Instance;
        public static IPoolManager PoolManager => EasyFramework.PoolManager.Instance;
        
        public static IAudioPlayer AudioPlayer => EasyFramework.AudioPlayer.Instance;

        private static bool _initialized;
        public static async ETask InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;

            // Init Editor Mode
#if UNITY_EDITOR
            EditorBridge.Initialize();
#endif
            Behaviour = FBehaviour.Instance;

            await ResLoader.InitializeAsync();
            
            AssetBundleLoader.Instance.ResRequestAliveTime = EasyFrameworkSettings.Instance.resRequestAliveTime;
            
            EasyFramework.WindowManager.CreateInstance();
            EasyFramework.SpriteLoader.CreateInstance();
            EasyFramework.ShaderLoader.CreateInstance();
            EasyFramework.SceneLoader.CreateInstance();
            EasyFramework.ControllerManager.CreateInstance();
            
            EasyFramework.AudioPlayer.CreateInstance();
            
            World.CreateSystem(typeof(F).Assembly);
        }
    }
}