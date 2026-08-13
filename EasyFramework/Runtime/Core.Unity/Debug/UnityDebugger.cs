
using System;
using UnityEngine;

namespace EasyFramework
{
    public class UnityDebugger : Singleton<UnityDebugger>, IDebugger
    {
        [HideInCallstack]
        public void Log(string message) => Debug.Log(message);
        [HideInCallstack]
        public void LogWarning(string message) => Debug.LogWarning(message);
        [HideInCallstack]
        public void LogError(string message) => Debug.LogError(message);
        [HideInCallstack]
        public void LogException(Exception exception) => Debug.LogException(exception);
    }
}