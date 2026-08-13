/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
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
        public static HybridCLRManager HybridCLRManager => HybridCLRManager.Instance;
        
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
}