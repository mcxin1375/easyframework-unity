/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public interface ITickerNode
    {
        bool OnTick();
        void OnRemove() { }
    }
    public partial class Ticker
    {
        private readonly HashSet<ITickerNode> _nodeHashSet = new();
        private readonly List<ITickerNode> _tickList = new();
        private readonly object _lock = new();
        private bool _refresh;

        
        private readonly Dictionary<Action, ActionNode> _actionDict = new();
        private readonly object _actionLock = new();
        
        private readonly Dictionary<Func<bool>, FuncNode> _funcDict = new();
        private readonly object _funcLock = new();
        
        public void Dispose()
        {
            _nodeHashSet.Clear();
            _tickList.Clear();
            _actionDict.Clear();
            _funcDict.Clear();
        }

        public void Tick()
        {
            lock (_lock)
            {
                if (_refresh)
                {
                    _refresh = false;
                    _tickList.Clear();
                    _tickList.AddRange(_nodeHashSet);
                }
            }

            foreach (var node in _tickList)
            {
                var alive = node.OnTick();
                if (!alive) Remove(node);
            }
        }
        public void Add(ITickerNode node)
        {
            if (node == null) return;
            lock (_lock)
            {
                if (!_nodeHashSet.Add(node)) return;
                _refresh = true;
            }
        }
        public void Remove(ITickerNode node)
        {
            if (node == null) return;
            lock (_lock)
            {
                _nodeHashSet.Remove(node);
                node.OnRemove();
                _refresh = true;
            }
        }
    }
}