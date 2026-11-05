using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EasyFramework.Profiler;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace EasyFramework.Tests
{
    public class ProfilerRecorderSamplingTests
    {
        [UnityTest]
        public IEnumerator Update_RealCounter_NonZeroToZeroToNonZero_UpdatesValueHeaderAndDetails()
        {
            var obj = new GameObject("profiler-counter-sampling-test");
            try
            {
                var panel = obj.AddComponent<ProfilerGUIBehaviour>();
                panel.enabled = false;
                var profiler = panel.ProfilerRecorderEx;
                var recorders = GetRecorders(profiler);
                var recorder = recorders[nameof(ProfilerRecorderEx.TotalReservedMemory)];
                Assert.That(recorder.Valid, Is.True, "Total Reserved Memory must be available in this Unity run.");
                Assert.That(recorder.IsRunning, Is.True);

                for (var frame = 0; frame < 120 && recorder.LastValue == 0; frame++) yield return null;
                Assert.That(recorder.LastValue, Is.GreaterThan(0), "The real counter did not publish a nonzero frame.");
                Assert.That(recorder.Count, Is.GreaterThan(0));
                UpdatePanel(panel);
                AssertValueAndText(panel, recorder.LastValue);

                // Unity Reset stops collection as well as clearing samples. Keep its
                // valid zero visible, then explicitly restart the same native recorder.
                recorder.Reset();
                Assert.That(recorder.IsRunning, Is.False);
                Assert.That(recorder.Count, Is.Zero);
                Assert.That(recorder.LastValue, Is.Zero);
                UpdatePanel(panel);
                AssertValueAndText(panel, 0);
                Assert.That(profiler.IsAvailable(nameof(ProfilerRecorderEx.TotalReservedMemory)), Is.True);

                recorder.Start();
                Assert.That(recorder.IsRunning, Is.True);
                Assert.That(recorders[nameof(ProfilerRecorderEx.TotalReservedMemory)].IsRunning, Is.True,
                    "The panel and test must observe the same native recorder handle.");
                for (var frame = 0; frame < 120 && recorder.LastValue == 0; frame++) yield return null;
                Assert.That(recorder.LastValue, Is.GreaterThan(0),
                    $"The real counter did not resume after Start. Running={recorder.IsRunning}, Count={recorder.Count}, Capacity={recorder.Capacity}.");
                Assert.That(recorder.Count, Is.GreaterThan(0));
                UpdatePanel(panel);
                AssertValueAndText(panel, recorder.LastValue);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Update_UnavailableCounter_ShowsUnavailableInHeaderAndDetails()
        {
            var obj = new GameObject("profiler-counter-unavailable-test");
            try
            {
                var panel = obj.AddComponent<ProfilerGUIBehaviour>();
                panel.enabled = false;
                var profiler = panel.ProfilerRecorderEx;
                var recorders = GetRecorders(profiler);
                recorders[nameof(ProfilerRecorderEx.TotalReservedMemory)].Dispose();
                recorders[nameof(ProfilerRecorderEx.TotalReservedMemory)] = default;

                UpdatePanel(panel);

                Assert.That(profiler.TotalReservedMemory, Is.Zero);
                Assert.That(profiler.IsAvailable(nameof(ProfilerRecorderEx.TotalReservedMemory)), Is.False);
                var usedMemory = profiler.FormatByte(nameof(ProfilerRecorderEx.TotalUsedMemory));
                StringAssert.Contains($" M(U/R): {usedMemory}/N/A", GetPanelText(panel, "_headerStr"));
                StringAssert.Contains($"TotalMemory(Used/Reserve): {usedMemory} / N/A\n", GetPanelText(panel, "_memoryStr"));
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        private static void AssertValueAndText(ProfilerGUIBehaviour panel, long expected)
        {
            var profiler = panel.ProfilerRecorderEx;
            Assert.That(profiler.TotalReservedMemory, Is.EqualTo(expected));
            Assert.That(profiler.FormatByte(nameof(ProfilerRecorderEx.TotalReservedMemory)),
                Is.EqualTo(FormatHelper.FormatByte(expected)));
            var usedMemory = profiler.FormatByte(nameof(ProfilerRecorderEx.TotalUsedMemory));
            StringAssert.Contains($" M(U/R): {usedMemory}/{FormatHelper.FormatByte(expected)}", GetPanelText(panel, "_headerStr"));
            StringAssert.Contains($"TotalMemory(Used/Reserve): {usedMemory} / {FormatHelper.FormatByte(expected)}\n", GetPanelText(panel, "_memoryStr"));
            Assert.That(GetPanelText(panel, "_memoryStr"), Is.EqualTo(profiler.MemoryToString()));
            Assert.That(GetPanelText(panel, "_renderStr"), Is.EqualTo(profiler.RenderToString()));
        }

        private static Dictionary<string, ProfilerRecorder> GetRecorders(ProfilerRecorderEx profiler)
        {
            return (Dictionary<string, ProfilerRecorder>)typeof(ProfilerRecorderEx)
                .GetField("_profilerRecorderDict", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(profiler);
        }

        private static void UpdatePanel(ProfilerGUIBehaviour panel)
        {
            typeof(ProfilerGUIBehaviour).GetField("_time", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(panel, -1f);
            typeof(ProfilerGUIBehaviour).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(panel, null);
        }

        private static string GetPanelText(ProfilerGUIBehaviour panel, string fieldName)
        {
            return (string)typeof(ProfilerGUIBehaviour).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(panel);
        }
    }
}
