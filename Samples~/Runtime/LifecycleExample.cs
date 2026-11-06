using System;

namespace EasyFramework.Samples.Modules
{
    // 在 Unity 主线程持有实例并 await SubscribeAsync()；业务结束时 Dispose()，不要销毁共享宿主。
    // Add/Remove 下次 Update 生效；Remove 不调用 OnDestroy，await 期间 Dispose 后不会迟到注册。
    public sealed class LifecycleExample : IFrameworkTick, IDisposable
    {
        private bool _registered;
        private bool _disposed;
        public int UpdateCount { get; private set; }
        public int LateUpdateCount { get; private set; }

        public async ETask SubscribeAsync()
        {
            await F.InitializeAsync();
            if (_disposed || _registered) return;
            F.Behaviour.Add(this);
            _registered = true;
        }

        public void OnUpdate() => UpdateCount++;
        public void OnLateUpdate() => LateUpdateCount++;

        public void OnDestroy()
        {
            _registered = false;
            _disposed = true;
        }

        public void Dispose()
        {
            _disposed = true;
            if (!_registered) return;
            F.Behaviour.Remove(this);
            _registered = false;
        }
    }
}
