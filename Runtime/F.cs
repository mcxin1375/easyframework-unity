/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using System;
using System.Runtime.ExceptionServices;

namespace EasyFramework
{
    public static class F
    {
        public static EasyFrameworkSettings Settings => EasyFrameworkSettings.Instance;
        public static FBehaviour Behaviour => FBehaviour.Instance;

        public static IControllerManager ControllerManager => EasyFramework.ControllerManager.Instance;
        
        public static Event Event => Event.Instance;
        public static IDLCManager DLCManager => EasyFramework.DLCManager.Instance;
        public static IHttpManager HttpManager => EasyFramework.HttpManager.Instance;
        public static IWindowManager WindowManager => EasyFramework.WindowManager.Instance;
        
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
            Exception,
        }
        private static EState _state;

        public static async ETask InitializeAsync()
        {
            switch (_state)
            {
                case EState.None:
                case EState.Exception:
                    _state = EState.Initializing;
                    try
                    {
                        EasyFrameworkSettings.CreateInstance();
                        FDebug.Debugger = UnityDebugger.Instance;
                        FDebug.DebugLevel = EasyFrameworkSettings.Instance.debugLevel;
                        EasyFramework.WindowManager.CreateInstance();
                        EasyFramework.SpriteLoader.CreateInstance();
                        EasyFramework.ShaderLoader.CreateInstance();
                        EasyFramework.SceneLoader.CreateInstance();
                        EasyFramework.AudioPlayer.CreateInstance();
                        await EasyFramework.DLCManager.Instance.InitializeAsync();
                        await EasyFramework.AssetBundleLoader.Instance.InitializeAsync();
                        _state = EState.Initialized;
                    }
                    catch (Exception exception)
                    {
                        FDebug.LogException(exception);
                        _state = EState.Exception;
                        throw;
                    }

                    break;
                case EState.Initializing:
                    await ETask.WaitUntil(() => _state != EState.Initializing);
                    if (_state == EState.Exception) throw new Exception($"F.InitializeAsync Error!");
                    break;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitOnLoadMethod()
        {
            var settings = EasyFrameworkSettings.CreateInstance();
            if (settings.autoInitialize) InitializeAsync().Forget();
        }
    }
}
