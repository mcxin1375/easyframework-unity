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
        public void Add(Func<bool> func)
        {
            if (func == null) return;
            
            lock (_funcLock)
            {
                var node = FuncNode.Create(this, func);
                if (!_funcDict.TryAdd(func, node))
                {
                    node.Stop();
                    node.OnRemove();
                    return;
                }
                Add(node);
            }
        }
        public void Remove(Func<bool> func)
        {
            if (func == null) return;
            
            lock (_funcLock)
            {
                if (_funcDict.Remove(func, out var node))
                    node.Stop();
            }
        }

        class FuncNode : ITickerNode
        {
            private Ticker _ticker;
            private Func<bool> _func;
            private bool _alive;
            
            public bool OnTick()
            {
                if (_func == null || !_alive) return false;
                return _func();
            }

            public void OnRemove()
            {
                _ticker.Remove(_func);
                ObjectPool<FuncNode>.Shared.Return(this);
            }

            public void Stop()
            {
                _alive = false;
                _func = null;
            }

            public static FuncNode Create(Ticker ticker,Func<bool> func)
            {
                var source = ObjectPool<FuncNode>.Shared.Rent();
                source._ticker = ticker;
                source._func = func;
                source._alive = true;
                return source;
            }
        }
    }
}