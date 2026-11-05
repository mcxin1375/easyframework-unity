using System.Collections.Generic;
using System.Reflection;
using EasyFramework.Profiler;
using NUnit.Framework;
using Unity.Profiling;

namespace EasyFramework.Tests
{
    public class ProfilerRecorderTests
    {
        [TestCase(nameof(ProfilerRecorderEx.DrawCallsCount))]
        [TestCase(nameof(ProfilerRecorderEx.GCAllocatedInFrame))]
        public void OnUpdate_ValidZero_ReplacesPreviousValue(string propertyName)
        {
            var profiler = new ProfilerRecorderEx();
            try
            {
                var recorders = GetRecorders(profiler);
                DisposeAndClear(recorders);
                bool renderCounter = propertyName == nameof(ProfilerRecorderEx.DrawCallsCount);
                var recorder = renderCounter
                    ? ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count")
                    : ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
                if (!renderCounter) recorder.Reset();
                recorders.Add(propertyName, recorder);
                Assert.That(recorder.Valid, Is.True, "The test requires a real available Unity counter.");
                Assert.That(renderCounter ? recorder.CurrentValue : recorder.LastValue, Is.Zero,
                    "The test must exercise a valid zero value.");
                SetValue(profiler, propertyName, 123);

                if (renderCounter) profiler.OnRender();
                else profiler.OnUpdate();

                Assert.That(GetValue(profiler, propertyName), Is.Zero);
                Assert.That(profiler.IsAvailable(propertyName), Is.True);
                Assert.That(profiler.FormatNumber(propertyName), Is.EqualTo("0"));
                Assert.That(profiler.FormatByte(propertyName), Is.EqualTo("0B"));
                var text = propertyName == nameof(ProfilerRecorderEx.DrawCallsCount)
                    ? profiler.RenderToString()
                    : profiler.MemoryToString();
                StringAssert.DoesNotContain("123", text);
            }
            finally
            {
                profiler.Dispose();
            }
        }

        [Test]
        public void OnUpdate_UnavailableCounter_ClearsOldValueAndShowsUnavailable()
        {
            var profiler = new ProfilerRecorderEx();
            try
            {
                var recorders = GetRecorders(profiler);
                DisposeAndClear(recorders);
                recorders.Add(nameof(ProfilerRecorderEx.DrawCallsCount), default);
                SetValue(profiler, nameof(ProfilerRecorderEx.DrawCallsCount), 123);

                profiler.OnUpdate();
                profiler.OnRender();

                Assert.That(profiler.DrawCallsCount, Is.Zero);
                Assert.That(profiler.IsAvailable(nameof(ProfilerRecorderEx.DrawCallsCount)), Is.False);
                Assert.That(profiler.FormatNumber(nameof(ProfilerRecorderEx.DrawCallsCount)), Is.EqualTo("N/A"));
                StringAssert.Contains("Draw Calls: N/A", profiler.RenderToString());
                StringAssert.Contains("TotalMemory(Used/Reserve): N/A / N/A", profiler.MemoryToString());
            }
            finally
            {
                profiler.Dispose();
            }
        }

        [Test]
        public void OnUpdate_PreservesRenderSnapshotUntilAnotherRenderCompletes()
        {
            var profiler = new ProfilerRecorderEx();
            try
            {
                SetValue(profiler, nameof(ProfilerRecorderEx.DrawCallsCount), 123);
                profiler.OnUpdate();
                Assert.That(profiler.DrawCallsCount, Is.EqualTo(123),
                    "An Editor Update without rendering must not clear the last rendered snapshot.");
            }
            finally { profiler.Dispose(); }
        }

        [Test]
        public void CounterSampling_AfterWarmup_DoesNotAllocateManagedMemory()
        {
            var profiler = new ProfilerRecorderEx();
            try
            {
                for (int i = 0; i < 10; i++) { profiler.OnUpdate(); profiler.OnRender(); }
                long before = System.GC.GetAllocatedBytesForCurrentThread();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                long afterWatch = System.GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++) { profiler.OnUpdate(); profiler.OnRender(); }
                long allocated = System.GC.GetAllocatedBytesForCurrentThread() - afterWatch;
                watch.Stop();
                TestContext.WriteLine($"1000 counter sampling pairs: {allocated} bytes, {watch.Elapsed.TotalMilliseconds:F2} ms; timer allocation={afterWatch - before} bytes.");
                Assert.That(allocated, Is.Zero, "Sampling must not box values or invoke reflection setters.");
            }
            finally { profiler.Dispose(); }
        }

        [Test]
        public void Dispose_MakesCounterUnavailable_AndRepeatedCleanupIsSafe()
        {
            var profiler = new ProfilerRecorderEx();
            try
            {
                Assert.That(profiler.IsAvailable(nameof(ProfilerRecorderEx.TotalReservedMemory)), Is.True);

                profiler.Dispose();
                profiler.Dispose();
                profiler.OnUpdate();

                Assert.That(profiler.TotalReservedMemory, Is.Zero);
                Assert.That(profiler.IsAvailable(nameof(ProfilerRecorderEx.TotalReservedMemory)), Is.False);
                Assert.That(profiler.FormatByte(nameof(ProfilerRecorderEx.TotalReservedMemory)), Is.EqualTo("N/A"));
            }
            finally
            {
                profiler.Dispose();
            }
        }

        private static Dictionary<string, ProfilerRecorder> GetRecorders(ProfilerRecorderEx profiler)
        {
            return (Dictionary<string, ProfilerRecorder>)typeof(ProfilerRecorderEx)
                .GetField("_profilerRecorderDict", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(profiler);
        }

        private static void DisposeAndClear(Dictionary<string, ProfilerRecorder> recorders)
        {
            foreach (var recorder in recorders.Values) recorder.Dispose();
            recorders.Clear();
        }

        private static void SetValue(ProfilerRecorderEx profiler, string propertyName, long value)
        {
            typeof(ProfilerRecorderEx).GetProperty(propertyName).SetValue(profiler, value);
        }

        private static long GetValue(ProfilerRecorderEx profiler, string propertyName)
        {
            return (long)typeof(ProfilerRecorderEx).GetProperty(propertyName).GetValue(profiler);
        }
    }
}
