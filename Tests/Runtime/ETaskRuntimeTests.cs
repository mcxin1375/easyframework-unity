using System.Collections;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EasyFramework.Tests
{
    public class ETaskRuntimeTests
    {
        private static readonly MethodInfo TickMethod = typeof(ETask).GetMethod("Tick", BindingFlags.Static | BindingFlags.NonPublic);

        [UnityTest]
        public IEnumerator DelayClockTypes_RespectTimeScale()
        {
            float originalTimeScale = Time.timeScale;
            var cancellation = new CancellationTokenSource();
            try
            {
                Time.timeScale = 0;
                float startUnscaledTime = Time.unscaledTime;
                float startRealtime = Time.realtimeSinceStartup;
                var scaled = ETask.DelaySeconds(0.1f, ETaskDelayType.Scaled, cancellation.Token);
                var unscaled = ETask.DelaySeconds(0.1f, ETaskDelayType.Unscaled, cancellation.Token);
                var realtime = ETask.DelaySeconds(0.1f, ETaskDelayType.Realtime, cancellation.Token);

                yield return new WaitForSecondsRealtime(0.15f);
                TickMethod.Invoke(null, null);

                Assert.That(scaled.Status, Is.EqualTo(ETaskStatus.Pending));
                Assert.That(unscaled.Status, Is.EqualTo(ETaskStatus.Succeeded),
                    $"unscaledTime {startUnscaledTime} -> {Time.unscaledTime}; realtime {startRealtime} -> {Time.realtimeSinceStartup}; thread={ETask.ThreadType}");
                Assert.That(realtime.Status, Is.EqualTo(ETaskStatus.Succeeded),
                    $"realtime {startRealtime} -> {Time.realtimeSinceStartup}; thread={ETask.ThreadType}");
                unscaled.GetAwaiter().GetResult();
                realtime.GetAwaiter().GetResult();

                Time.timeScale = originalTimeScale;
                cancellation.Cancel();
                yield return new WaitForSecondsRealtime(0.02f);
                TickMethod.Invoke(null, null);
                Assert.That(scaled.Status, Is.EqualTo(ETaskStatus.Canceled));
                Assert.That(Catch(() => scaled.GetAwaiter().GetResult()), Is.TypeOf<System.OperationCanceledException>());
            }
            finally
            {
                Time.timeScale = originalTimeScale;
                cancellation.Dispose();
            }
        }

        private static System.Exception Catch(System.Action action)
        {
            try { action(); return null; }
            catch (System.Exception exception) { return exception; }
        }
    }
}
