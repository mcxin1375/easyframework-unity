/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/25
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EasyFramework.Profiler
{
    public class ProfilerManager : SingletonBehaviour<ProfilerManager>
    {
        public event Action<ShaderVariantInfo> OnShaderVariantError;

        private readonly HashSet<string> _ignoreHashSet = new();
        private ConcurrentQueue<LogMessage> _pendingLogs = new();
        private ProfilerSettings _settings;
        private bool _profilerEnabled;
        private bool _errorGUIEnabled;
        private bool _multipleCameraEnabled;
        private bool _blockUIInputEnabled;

        private ProfilerSettings Settings => _settings != null ? _settings : _settings = ProfilerSettings.Instance;

        public bool ProfilerGUIBehaviour
        {
            get => Settings.profilerGUIBehaviour;
            set
            {
                Settings.profilerGUIBehaviour = value;
                SetProfilerEnabled(value);
            }
        }

        public bool MultipleCameraDebugBehaviour
        {
            get => Settings.multipleCameraDebugBehaviour;
            set
            {
                Settings.multipleCameraDebugBehaviour = value;
                SetMultipleCameraEnabled(value);
            }
        }

        public bool ErrorGUIBehaviour
        {
            get => Settings.errorGUIBehaviour;
            set
            {
                Settings.errorGUIBehaviour = value;
                SetErrorGUIEnabled(value);
            }
        }

        public bool BlockUIInput
        {
            get => Settings.blockUIInput;
            set
            {
                Settings.blockUIInput = value;
                _blockUIInputEnabled = value;
                GetComponent<ProfilerInputBlocker>()?.Refresh();
            }
        }

        private void Awake()
        {
            SetSingleton(this);
            _settings = ProfilerSettings.Instance;
            _profilerEnabled = !_settings.profilerGUIBehaviour;
            _errorGUIEnabled = !_settings.errorGUIBehaviour;
            _multipleCameraEnabled = !_settings.multipleCameraDebugBehaviour;
            _blockUIInputEnabled = _settings.blockUIInput;
            ApplySettings();
            Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
        }

        private void OnDestroy()
        {
            Interlocked.Exchange(ref _pendingLogs, null);
            Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
        }

        private void Update()
        {
            ApplySettings();
            ProcessLogs();
        }

        private void ApplySettings()
        {
            if (_profilerEnabled != Settings.profilerGUIBehaviour ||
                (Settings.profilerGUIBehaviour && GetComponent<ProfilerGUIBehaviour>() == null))
                SetProfilerEnabled(Settings.profilerGUIBehaviour);
            if (_multipleCameraEnabled != Settings.multipleCameraDebugBehaviour ||
                (Settings.multipleCameraDebugBehaviour && GetComponent<MultipleCameraDebugBehaviour>() == null))
                SetMultipleCameraEnabled(Settings.multipleCameraDebugBehaviour);
            if (_errorGUIEnabled != Settings.errorGUIBehaviour)
                SetErrorGUIEnabled(Settings.errorGUIBehaviour);
            if (_blockUIInputEnabled != Settings.blockUIInput)
            {
                _blockUIInputEnabled = Settings.blockUIInput;
                GetComponent<ProfilerInputBlocker>()?.Refresh();
            }
        }

        private void SetProfilerEnabled(bool enabled)
        {
            _profilerEnabled = enabled;
            if (enabled) gameObject.AddComponentEx<ProfilerGUIBehaviour>();
            else gameObject.RemoveComponentEx<ProfilerGUIBehaviour>();
        }

        private void SetMultipleCameraEnabled(bool enabled)
        {
            _multipleCameraEnabled = enabled;
            if (enabled) gameObject.AddComponentEx<MultipleCameraDebugBehaviour>();
            else gameObject.RemoveComponentEx<MultipleCameraDebugBehaviour>();
        }

        private void SetErrorGUIEnabled(bool enabled)
        {
            _errorGUIEnabled = enabled;
            if (enabled) gameObject.AddComponentEx<ErrorGUIBehaviour>();
        }

        internal void SetInputBlocked(MonoBehaviour owner, bool visible)
        {
            _blockUIInputEnabled = Settings.blockUIInput;
            var blocker = GetComponent<ProfilerInputBlocker>();
            if (blocker == null)
            {
                if (!visible) return;
                blocker = gameObject.AddComponentEx<ProfilerInputBlocker>();
            }
            blocker.SetOwner(owner, visible);
        }

        internal void OnException(string condition, string stackTrace, LogType type)
        {
            if (Settings.errorGUIBehaviour)
                GetComponent<ErrorGUIBehaviour>()?.AddLog(condition, stackTrace, type);
        }

        private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    Volatile.Read(ref _pendingLogs)?.Enqueue(new LogMessage(condition, stackTrace, type));
                    break;
            }
        }

        private void ProcessLogs()
        {
            var queue = Volatile.Read(ref _pendingLogs);
            if (queue == null) return;

            int count = queue.Count;
            for (int index = 0; index < count && ReferenceEquals(queue, _pendingLogs) && queue.TryDequeue(out var log); index++)
            {
                if (_ignoreHashSet.Contains(log.Condition)) continue;
                bool shaderError = ShaderVariantInfo.TryParseFromLog(log.Condition, out var shaderVariantInfo);
                if (shaderError) _ignoreHashSet.Add(log.Condition);
                OnException(log.Condition, log.StackTrace, log.Type);
                if (!shaderError || OnShaderVariantError == null) continue;

                foreach (Action<ShaderVariantInfo> handler in OnShaderVariantError.GetInvocationList())
                {
                    if (!ReferenceEquals(queue, _pendingLogs)) break;
                    try { handler(shaderVariantInfo); }
                    catch (Exception exception) { FDebug.LogException(exception); }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            if (ProfilerSettings.Instance.autoInitialize) CreateInstance();
        }

        private readonly struct LogMessage
        {
            public readonly string Condition;
            public readonly string StackTrace;
            public readonly LogType Type;

            public LogMessage(string condition, string stackTrace, LogType type)
            {
                Condition = condition;
                StackTrace = stackTrace;
                Type = type;
            }
        }
    }
}
