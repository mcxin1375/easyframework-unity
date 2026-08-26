/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public partial class Ticker
    {
        public void Add(Action action, bool loop)
        {
            if (action == null) return;

            lock (_actionLock)
            {
                var node = ActionNode.Create(this, action, loop);
                if (!_actionDict.TryAdd(action, node))
                {
                    node.Stop();
                    node.OnRemove();
                    return;
                }
                Add(node);
            }
        }
        public void Remove(Action action)
        {
            if (action == null) return;

            lock (_actionLock)
            {
                if (_actionDict.Remove(action, out var node))
                    node.Stop();
            }
        }

        class ActionNode : ITickerNode
        {
            private Ticker _ticker;
            private Action _action;
            private bool _loop;
            private bool _alive;
            
            public bool OnTick()
            {
                if (_action == null || !_alive) return false;
                _action?.Invoke();
                return _loop;
            }

            public void OnRemove()
            {
                _ticker.Remove(_action);
                ObjectPool<ActionNode>.Shared.Return(this);
            }

            public void Stop()
            {
                _alive = false;
                _action = null;
            }

            public static ActionNode Create(Ticker ticker, Action action, bool loop)
            {
                var source = ObjectPool<ActionNode>.Shared.Rent();
                source._ticker = ticker;
                source._action = action;
                source._loop = loop;
                source._alive = true;
                return source;
            }
        }
    }
}