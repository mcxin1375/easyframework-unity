/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/25
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework.Profiler
{
    public class ProfilerSystem : FSystem
    {
        public event Action<ShaderVariantInfo> OnShaderVariantError;
        
        public bool ProfilerGUIBehaviour
        {
            set
            {
                if (!ProfilerBehaviour.HasInstance()) return;
                if (value) ProfilerBehaviour.Instance.gameObject.AddComponentEx<ProfilerGUIBehaviour>();
                else ProfilerBehaviour.Instance.gameObject.RemoveComponentEx<ProfilerGUIBehaviour>();
            }
        }
        public bool MultipleCameraDebugBehaviour
        {
            set
            {
                if (!ProfilerBehaviour.HasInstance()) return;
                if (value) ProfilerBehaviour.Instance.gameObject.AddComponentEx<MultipleCameraDebugBehaviour>();
                else ProfilerBehaviour.Instance.gameObject.RemoveComponentEx<MultipleCameraDebugBehaviour>();
            }
        }
        public bool ErrorGUIBehaviour
        {
            set
            {
                if (!ProfilerBehaviour.HasInstance()) return;
                FProfiler.Settings.errorGUIBehaviour = value;
            }
        }
        
        private readonly HashSet<string> _ignoreHashSet = new();

        protected override void OnCreate()
        {
            Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
            
            var gameObject = ProfilerBehaviour.Instance.gameObject;
            if (FProfiler.Settings.profilerGUIBehaviour) gameObject.AddComponentEx<ProfilerGUIBehaviour>();
            if (FProfiler.Settings.multipleCameraDebugBehaviour) gameObject.AddComponentEx<MultipleCameraDebugBehaviour>();
        }

        protected override void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
        }
        
        private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            if (_ignoreHashSet.Contains(condition)) return;
            
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    if (ShaderVariantInfo.TryParseFromLog(condition, out var shaderVariantInfo))
                    {
                        _ignoreHashSet.Add(condition); // Shader变体错误只处理一次
                        OnShaderVariantError?.Invoke(shaderVariantInfo);
                    }
                    if (ProfilerBehaviour.HasInstance())
                    {
                        ProfilerBehaviour.Instance.OnException(condition, stackTrace, type);
                    }
                    break;
            }
        }
    }
}