
using System;
using System.Collections.Generic;

namespace EasyFramework
{
    [Flags]
    public enum EDebugLevel
    {
        Log = 1,
        LogWarning = 2,
        LogError = 4
    }

    public static class FDebug
    {
        private static EDebugLevel _debugLevel = EDebugLevel.Log | EDebugLevel.LogWarning | EDebugLevel.LogError;
        private static bool _logEnabled = true;
        private static bool _logWarningEnabled = true;
        private static bool _logErrorEnabled = true;

        public static EDebugLevel DebugLevel
        {
            get => _debugLevel;
            set
            {
                _debugLevel = value;
                _logEnabled = (_debugLevel & EDebugLevel.Log) > 0;
                _logWarningEnabled = (_debugLevel & EDebugLevel.LogWarning) > 0;
                _logErrorEnabled = (_debugLevel & EDebugLevel.LogError) > 0;
            }
        }

        public static IDebugger Debugger { get; set; } = new UnityDebugger();

#if UNITY_EDITOR
        [UnityEngine.HideInCallstack]
#endif
        public static void Log(object message) => Log(message.ToString());
#if UNITY_EDITOR
        [UnityEngine.HideInCallstack]
#endif
        public static void Log(string message)
        {
            if (_logEnabled) Debugger.Log(message);
        }

#if UNITY_EDITOR
        [UnityEngine.HideInCallstack]
#endif
        public static void LogWarning(object message) => LogWarning(message.ToString());
#if UNITY_EDITOR
        [UnityEngine.HideInCallstack]
#endif
        public static void LogWarning(string message)
        {
            if (_logWarningEnabled) Debugger.LogWarning(message);
        }


#if UNITY_EDITOR
        [UnityEngine.HideInCallstack]
#endif
        public static void LogError(object message) => LogError(message.ToString());
#if UNITY_EDITOR
        [UnityEngine.HideInCallstack]
#endif
        public static void LogError(string message)
        {
            if (_logErrorEnabled) Debugger.LogError(message);
        }
        
#if UNITY_EDITOR
        [UnityEngine.HideInCallstack]
#endif
        public static void LogException(Exception exception)
        {
            Debugger.LogException(exception);
        }
        

        public static TimeDebug StartTime(string tag = null) => TimeDebug.Start(tag);
        
        public static IReadOnlyList<IPoolDebug> PoolDebugListRO => PoolDebugList;
        private static readonly List<IPoolDebug> PoolDebugList = new();

        public static void AddDebug(this IPoolDebug pool)
        {
            PoolDebugList.Add(pool);
        }
    }
}
