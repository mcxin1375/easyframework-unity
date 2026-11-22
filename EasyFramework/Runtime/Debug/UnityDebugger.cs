using System;
using UnityEngine;

namespace EasyFramework
{
    public class UnityDebugger : IDebugger
    {
#if UNITY_EDITOR && UNITY_6000_0_OR_NEWER
        [HideInCallstack]
#endif
        public void Log(string message) => Debug.Log(message);
#if UNITY_EDITOR && UNITY_6000_0_OR_NEWER
        [HideInCallstack]
#endif
        public void LogWarning(string message) => Debug.LogWarning(message);
#if UNITY_EDITOR && UNITY_6000_0_OR_NEWER
        [HideInCallstack]
#endif
        public void LogError(string message) => Debug.LogError(message);
#if UNITY_EDITOR && UNITY_6000_0_OR_NEWER
        [HideInCallstack]
#endif
        public void LogException(Exception exception) => Debug.LogException(exception);
    }
}
