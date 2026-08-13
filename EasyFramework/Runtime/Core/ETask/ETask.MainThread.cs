/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Threading;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        interface IThread
        {
            int ThreadId { get; }
            float Time { get; }
        }

#if UNITY_2022_1_OR_NEWER
        public class UnityThread : IThread
        {
            public int ThreadId { get; }
            public float Time => UnityEngine.Time.time;

            public UnityThread()
            {
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                
                if (!UnityEngine.Application.isPlaying) return;
                EasyFrameworkLoopSystem.Add(UnityLoopTiming.Update, OnUnityUpdate);
            }

            private void OnUnityUpdate()
            {
                if (!UnityEngine.Application.isPlaying) return;
                
                // UnityEngine.Debug.Log("OnUnityUpdate");
                ETask.Tick();
            }
        }
#endif


        class ETaskThread : IThread
        {
            int IThread.ThreadId => _thread?.ManagedThreadId ?? -1;
            float IThread.Time => _time;

            private const int FPS = 30;
            private const long TickInterval = (long)(1f / FPS * 1000);

            private bool _running;
            private float _time = 0;
            private readonly Thread _thread;

            public ETaskThread()
            {
                _running = true;
                _thread = new Thread(ThreadAction)
                {
                    IsBackground = true, // 后台线程，随进程结束
                    Name = nameof(ETaskThread)
                };
                _thread.Start();
                AppDomain.CurrentDomain.ProcessExit += (_, _) => { OnProcessExit(); };
            }

            private void OnProcessExit()
            {
                _running = false;
                ETask.Dispose();
            }

            private void ThreadAction()
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                long nextTick = 0;
                while (_running)
                {
                    try
                    {
                        ETask.Tick();
                    }
                    catch (Exception ex)
                    {
                        FDebug.LogError(ex.ToString());
                    }

                    nextTick += TickInterval;
                    long sleep = nextTick - sw.ElapsedMilliseconds;
                    if (sleep > 0)
                        Thread.Sleep((int)sleep);
                    else
                        nextTick = sw.ElapsedMilliseconds;
                    _time = sw.ElapsedMilliseconds / 1000f;
                }

                sw.Stop();
            }
        }
    }
}