using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EasyFramework.Profiler;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace EasyFramework.Tests
{
    public class ProfilerManagerTests
    {
        private static readonly Type HostType = typeof(ProfilerManager);
        private ProfilerSettings _settings;
        private bool _autoInitialize, _profilerGUI, _errorGUI, _multipleCamera;
        private IDebugger _debugger;
        private QuietDebugger _quietDebugger;

        [SetUp]
        public void Setup()
        {
            CleanupManager();
            _settings = ProfilerSettings.Instance;
            _autoInitialize = _settings.autoInitialize;
            _profilerGUI = _settings.profilerGUIBehaviour;
            _errorGUI = _settings.errorGUIBehaviour;
            _multipleCamera = _settings.multipleCameraDebugBehaviour;
            _settings.autoInitialize = false;
            _settings.profilerGUIBehaviour = false;
            _settings.errorGUIBehaviour = true;
            _settings.multipleCameraDebugBehaviour = false;
            _debugger = FDebug.Debugger;
            FDebug.Debugger = _quietDebugger = new QuietDebugger();
        }

        [TearDown]
        public void Cleanup()
        {
            CleanupManager();
            _settings.autoInitialize = _autoInitialize;
            _settings.profilerGUIBehaviour = _profilerGUI;
            _settings.errorGUIBehaviour = _errorGUI;
            _settings.multipleCameraDebugBehaviour = _multipleCamera;
            FDebug.Debugger = _debugger;
        }

        [Test]
        public void AutoInitializeFalse_DoesNotCreateManagerHostOrRecorders()
        {
            InitializeAutomatically();

            Assert.That(ProfilerManager.HasInstance(), Is.False);
            Assert.That(Object.FindObjectsOfType(HostType), Is.Empty);
            Assert.That(Object.FindObjectsOfType<ProfilerGUIBehaviour>(), Is.Empty);
        }

        [Test]
        public void AutoInitializeTrue_StartsConfiguredPanelOnce()
        {
            _settings.autoInitialize = true;
            _settings.profilerGUIBehaviour = true;

            InitializeAutomatically();
            InitializeAutomatically();

            Assert.That(ProfilerManager.HasInstance(), Is.True);
            Assert.That(Object.FindObjectsOfType(HostType).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsOfType<ProfilerGUIBehaviour>().Length, Is.EqualTo(1));
        }

        [Test]
        public void AutoInitializeFalse_AllowsExplicitStartup()
        {
            InitializeAutomatically();
            var manager = ProfilerManager.CreateInstance();

            Assert.That(manager, Is.Not.Null);
            Assert.That(Object.FindObjectsOfType(HostType).Length, Is.EqualTo(1));
            Assert.That(ProfilerManager.CreateInstance(), Is.SameAs(manager));
        }

        [UnityTest]
        public IEnumerator SettingsChanges_AddAndRemoveManagedModules()
        {
            var manager = ProfilerManager.CreateInstance();
            Assert.That(manager.GetComponent<ProfilerGUIBehaviour>(), Is.Null);
            Assert.That(manager.GetComponent<ErrorGUIBehaviour>(), Is.Not.Null);
            Assert.That(manager.GetComponent<MultipleCameraDebugBehaviour>(), Is.Null);

            _settings.profilerGUIBehaviour = true;
            _settings.errorGUIBehaviour = false;
            _settings.multipleCameraDebugBehaviour = true;
            yield return null;

            Assert.That(manager.GetComponent<ProfilerGUIBehaviour>(), Is.Not.Null);
            Assert.That(manager.GetComponent<ErrorGUIBehaviour>(), Is.Not.Null, "Disabling error capture must retain existing logs and panel.");
            Assert.That(manager.GetComponent<MultipleCameraDebugBehaviour>(), Is.Not.Null);

            _settings.profilerGUIBehaviour = false;
            _settings.errorGUIBehaviour = true;
            _settings.multipleCameraDebugBehaviour = false;
            yield return null;

            Assert.That(manager.GetComponent<ProfilerGUIBehaviour>(), Is.Null);
            Assert.That(manager.GetComponent<ErrorGUIBehaviour>(), Is.Not.Null);
            Assert.That(manager.GetComponent<MultipleCameraDebugBehaviour>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator SameFrameOffThenOn_RecreatesModulesAfterDeferredDestroy()
        {
            var manager = ProfilerManager.CreateInstance();
            manager.ProfilerGUIBehaviour = true;
            manager.MultipleCameraDebugBehaviour = true;
            manager.ProfilerGUIBehaviour = false;
            manager.MultipleCameraDebugBehaviour = false;
            manager.ProfilerGUIBehaviour = true;
            manager.MultipleCameraDebugBehaviour = true;
            yield return null;
            yield return null;

            Assert.That(manager.GetComponents<ProfilerGUIBehaviour>().Length, Is.EqualTo(1));
            Assert.That(manager.GetComponents<MultipleCameraDebugBehaviour>().Length, Is.EqualTo(1));
        }

        [Test]
        public void ThreadedLogs_AreDeferredAndDeduplicatedOnMainThread()
        {
            var manager = ProfilerManager.CreateInstance();
            int mainThread = Thread.CurrentThread.ManagedThreadId;
            var notificationThreads = new ConcurrentBag<int>();
            manager.OnShaderVariantError += _ => notificationThreads.Add(Thread.CurrentThread.ManagedThreadId);
            string shaderError = ShaderError("Threaded");
            LogAssert.Expect(LogType.Error, shaderError);
            LogAssert.Expect(LogType.Error, shaderError);
            LogAssert.Expect(LogType.Error, "Profiler ordinary worker error");
            var worker = Task.Run(() =>
            {
                Debug.LogError(shaderError);
                Debug.LogError(shaderError);
                Debug.LogError("Profiler ordinary worker error");
            });
            try
            {
                Assert.That(worker.Wait(TimeSpan.FromSeconds(5)), Is.True);
                Assert.That(notificationThreads, Is.Empty, "Threaded callback must only enqueue logs.");
                Assert.That(LogCount, Is.Zero, "The worker must not mutate the error panel stack.");
                DrainLogs();
                Assert.That(notificationThreads.ToArray(), Is.EqualTo(new[] { mainThread }));
                Assert.That(LogCount, Is.EqualTo(2), "A duplicate shader log is suppressed; ordinary errors remain visible.");
            }
            finally { worker.GetAwaiter().GetResult(); }
        }

        [Test]
        public void ConcurrentErrors_AllowMainThreadCloseAndClear()
        {
            var manager = ProfilerManager.CreateInstance();
            var threads = new ConcurrentBag<int>();
            manager.OnShaderVariantError += _ => threads.Add(Thread.CurrentThread.ManagedThreadId);
            int mainThread = Thread.CurrentThread.ManagedThreadId;
            var stressMessage = new Regex("^(Shader Stress[0-3], subshader 0, pass 0, stage fragment: variant TEST not found\\.|Profiler stress worker [0-3] error [0-9]+)$");
            for (int item = 0; item < 800; item++) LogAssert.Expect(LogType.Error, stressMessage);
            var workers = new Task[4];
            for (int index = 0; index < workers.Length; index++)
            {
                int workerIndex = index;
                workers[index] = Task.Run(() =>
                {
                    for (int item = 0; item < 100; item++)
                    {
                        Debug.LogError(ShaderError("Stress" + workerIndex));
                        Debug.LogError("Profiler stress worker " + workerIndex + " error " + item);
                    }
                });
            }
            var all = Task.WhenAll(workers);
            try
            {
                // Test Framework 1.1.33 checks its log scope outside the callback lock after
                // coroutine yields. Keep workers inside this synchronous scope so all logs
                // are received before the runner evaluates their expectations.
                var timeout = System.Diagnostics.Stopwatch.StartNew();
                while (!all.IsCompleted && timeout.Elapsed < TimeSpan.FromSeconds(10))
                {
                    DrainLogs();
                    InvokePanel("OnCloseLog");
                    InvokePanel("OnClearLog");
                    Thread.Yield();
                }
                Assert.That(all.IsCompleted, Is.True);
                all.GetAwaiter().GetResult();
                DrainLogs();

                Assert.That(threads.Count, Is.EqualTo(4));
                foreach (var thread in threads) Assert.That(thread, Is.EqualTo(mainThread));
                Assert.DoesNotThrow(() => InvokePanel("OnClearLog"));
                Assert.That(LogCount, Is.Zero);
            }
            finally { all.GetAwaiter().GetResult(); }
        }

        [Test]
        public void ShaderSubscriberFailure_DoesNotSkipOtherSubscribersOrLaterLogs()
        {
            var manager = ProfilerManager.CreateInstance();
            int notifications = 0;
            var failure = new InvalidOperationException("Profiler shader subscriber failed");
            manager.OnShaderVariantError += _ => throw failure;
            manager.OnShaderVariantError += _ => notifications++;
            Receive(manager, ShaderError("SubscriberA"));
            Receive(manager, ShaderError("SubscriberB"));

            Assert.DoesNotThrow(DrainLogs);
            Assert.That(notifications, Is.EqualTo(2));
            Assert.That(_quietDebugger.Exceptions, Is.EqualTo(2));
            Assert.That(LogCount, Is.EqualTo(2));
        }

        [Test]
        public void DestroyedManager_DropsLateCallbackAndCannotReachReplacement()
        {
            var manager = ProfilerManager.CreateInstance();
            int oldNotifications = 0;
            manager.OnShaderVariantError += _ => oldNotifications++;
            ProfilerManager.DestroyInstance();
            Receive(manager, ShaderError("Late"));
            var replacement = ProfilerManager.CreateInstance();
            int newNotifications = 0;
            replacement.OnShaderVariantError += _ => newNotifications++;

            DrainLogs();
            Assert.That(oldNotifications, Is.Zero);
            Assert.That(newNotifications, Is.Zero);
            Assert.That(LogCount, Is.Zero);
        }

        [Test]
        public void MalformedShaderLog_DoesNotBlockFollowingLog()
        {
            var manager = ProfilerManager.CreateInstance();
            int notifications = 0;
            manager.OnShaderVariantError += _ => notifications++;
            Receive(manager, "Shader Bad, subshader 0, pass invalid, stage fragment: variant TEST not found.");
            Receive(manager, ShaderError("AfterMalformed"));

            Assert.DoesNotThrow(DrainLogs);
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(LogCount, Is.EqualTo(2));
        }

        private static string ShaderError(string name)
            => "Shader " + name + ", subshader 0, pass 0, stage fragment: variant TEST not found.";

        private static void InitializeAutomatically()
            => typeof(ProfilerManager).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);

        private static void Receive(ProfilerManager manager, string condition)
            => typeof(ProfilerManager).GetMethod("Application_logMessageReceivedThreaded", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(manager, new object[] { condition, "test stack", LogType.Error });

        private static Component Host => Object.FindObjectOfType<ProfilerManager>();
        private static ErrorGUIBehaviour Panel => Host.GetComponent<ErrorGUIBehaviour>();
        private static int LogCount
        {
            get
            {
                var stack = typeof(ErrorGUIBehaviour).GetField("_logStack", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Panel);
                return (int)stack.GetType().GetProperty("Count").GetValue(stack);
            }
        }

        private static void InvokePanel(string method)
            => typeof(ErrorGUIBehaviour).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Panel, null);

        private static void DrainLogs() => Host.SendMessage("Update", SendMessageOptions.DontRequireReceiver);

        private static void CleanupManager()
        {
            ProfilerManager.DestroyInstance();
            foreach (var host in Object.FindObjectsOfType<ProfilerManager>())
                if (host != null) Object.DestroyImmediate(host.gameObject);
        }

        private sealed class QuietDebugger : IDebugger
        {
            public int Exceptions;
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public void LogException(Exception exception) { Exceptions++; }
        }
    }
}
