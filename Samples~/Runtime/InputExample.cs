using System;
using UnityEngine;

namespace EasyFramework.Samples.Modules
{
    // 启用 Input Manager（Old）或 Both，在 Unity 主线程 await SubscribeAsync()，业务结束时 Dispose()。
    // 鼠标索引 1001～1003，触摸索引 fingerId+1；await 期间 Dispose 后不会迟到订阅。
    public sealed class InputExample : IDisposable
    {
        private bool _subscribed;
        private bool _disposed;

        public async ETask SubscribeAsync()
        {
            await F.InitializeAsync();
            if (_disposed || _subscribed) return;
            F.InputManager.OnInputEvent += OnInput;
            _subscribed = true;
        }

        private void OnInput(EInputType type, int index, Vector2 position)
        {
            Debug.Log($"输入：{type}，索引：{index}，屏幕坐标：{position}");
        }

        public void Dispose()
        {
            _disposed = true;
            if (!_subscribed) return;
            F.InputManager.OnInputEvent -= OnInput;
            _subscribed = false;
        }
    }
}
