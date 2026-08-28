/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public static class F
    {
        public static WorldManager WorldManager => EasyFramework.WorldManager.Instance;
        public static IControllerManager ControllerManager => EasyFramework.ControllerManager.Instance;
        
        public static EasyFrameworkSettings Settings => EasyFrameworkSettings.Instance;
        public static FBehaviour Behaviour { get; private set; }
        
        public static Event Event => Event.Instance;
        // public static LocalStorageManager LocalStorageManager => LocalStorageManager.Instance;
        public static MainResManager MainResManager => MainResManager.Instance;
        public static IDLCManager DLCManager => EasyFramework.DLCManager.Instance;
        public static IHttpManager HttpManager => EasyFramework.HttpManager.Instance;
        public static IWindowManager WindowManager => EasyFramework.WindowManager.Instance;
        
        public static DLCDownloader DLCDownloader => EasyFramework.DLCDownloader.Instance;
        public static ISpriteLoader SpriteLoader => EasyFramework.SpriteLoader.Instance;
        public static IShaderLoader ShaderLoader => EasyFramework.ShaderLoader.Instance;
        public static ISceneLoader SceneLoader => EasyFramework.SceneLoader.Instance;
        public static IInputManager InputManager => EasyFramework.InputManager.Instance;
        public static IPoolManager PoolManager => EasyFramework.PoolManager.Instance;
        
        public static IAudioPlayer AudioPlayer => EasyFramework.AudioPlayer.Instance;

        public static IResLoader ResLoader
        {
            get
            {
#if UNITY_EDITOR
                if (EasyFrameworkSettings.Instance.resLoaderEditorMode)
                {
                    return EditorBridge.ResLoader;
                }
                return AssetBundleLoader.Instance;
#endif
                return AssetBundleLoader.Instance;
            }
        }
        
#if EF_HYBRIDCLR
        public static HybridCLRManager HybridCLRManager => HybridCLRManager.Instance;
#endif
        
        private enum EState
        {
            None,
            Initializing,
            Initialized,
        }
        private static EState _state = EState.None;
        
        public static void Initialize()
        {
            _ = InitializeAsync();
        }
        public static async ETask InitializeAsync()
        {
            switch (_state)
            {
                case EState.None:
                    
                    _state = EState.Initializing;
            
                    EasyFrameworkSettings.CreateInstance();
                    FDebug.Debugger = UnityDebugger.Instance;
                    FDebug.DebugLevel = EasyFrameworkSettings.Instance.debugLevel;
            
                    Behaviour = FBehaviour.Instance;

                    EasyFramework.WindowManager.CreateInstance();
                    EasyFramework.SpriteLoader.CreateInstance();
                    EasyFramework.ShaderLoader.CreateInstance();
                    EasyFramework.SceneLoader.CreateInstance();
                    EasyFramework.AudioPlayer.CreateInstance();
                    
                    await EasyFramework.DLCManager.Instance.InitializeAsync();
                    await EasyFramework.AssetBundleLoader.Instance.InitializeAsync();
            
                    _state =  EState.Initialized;
                    
                    break;
                case EState.Initializing:
                    await ETask.WaitUntil(() => _state == EState.Initialized);
                    break;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitOnLoadMethod()
        {
            var settings = EasyFrameworkSettings.CreateInstance();
            if (!settings.autoInitialize) return;
            Initialize();
        }
    }
}