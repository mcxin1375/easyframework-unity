/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EasyFramework
{
    public interface ITickerNode
    {
        bool OnTick();
    }

    public class Ticker
    {
        private const int MaxPooledNodes = 1024;
        private readonly Dictionary<ITickerNode, Node> _nodes = new(NodeComparer.Instance);
        private readonly Dictionary<Action, Node> _actionDict = new();
        private readonly Dictionary<Func<bool>, Node> _funcDict = new();
        private readonly Stack<Node> _pool = new();
        private readonly object _lock = new();
        private Node _head, _tail;
        private Node _pendingHead, _pendingTail;
        private bool _dispatching;
        private bool _clearActive;

        public void Add(ITickerNode target)
        {
            if (target == null) return;
            lock (_lock)
            {
                if (_nodes.TryGetValue(target, out var previous)) previous.Expired = true;
                var node = RentLocked();
                node.Target = target;
                _nodes[target] = node;
                AppendPendingLocked(node);
            }
        }

        public void Add(Action action, bool loop)
        {
            if (action == null) return;
            lock (_lock)
            {
                if (_actionDict.TryGetValue(action, out var previous)) previous.Expired = true;
                var node = RentLocked();
                node.Action = action;
                node.Loop = loop;
                _actionDict[action] = node;
                AppendPendingLocked(node);
            }
        }

        public void Add(Func<bool> func)
        {
            if (func == null) return;
            lock (_lock)
            {
                if (_funcDict.TryGetValue(func, out var previous)) previous.Expired = true;
                var node = RentLocked();
                node.Func = func;
                _funcDict[func] = node;
                AppendPendingLocked(node);
            }
        }

        public void Remove(ITickerNode target)
        {
            if (target == null) return;
            lock (_lock)
            {
                if (_nodes.Remove(target, out var node)) node.Expired = true;
            }
        }

        public void Remove(Action action)
        {
            if (action == null) return;
            lock (_lock)
            {
                if (_actionDict.Remove(action, out var node)) node.Expired = true;
            }
        }

        public void Remove(Func<bool> func)
        {
            if (func == null) return;
            lock (_lock)
            {
                if (_funcDict.Remove(func, out var node)) node.Expired = true;
            }
        }

        public void Tick()
        {
            lock (_lock)
            {
                if (_dispatching) return;
                _dispatching = true;
                // Detach this batch. Registrations added by callbacks wait for the next tick.
                if (_pendingHead != null)
                {
                    if (_tail == null) _head = _pendingHead;
                    else _tail.Next = _pendingHead;
                    _pendingHead.Prev = _tail;
                    _tail = _pendingTail;
                    _pendingHead = _pendingTail = null;
                }
            }

            try
            {
                var node = _head;
                while (node != null)
                {
                    var next = node.Next;
                    bool keep = false;
                    if (!node.Expired)
                    {
                        try { keep = node.Invoke(); }
                        catch (Exception error)
                        {
                            FDebug.LogException(error);
                        }
                    }
                    // Only this registration is retired; its target may already be reused.
                    if (!keep || node.Expired) Retire(node);
                    node = next;
                }
            }
            finally
            {
                lock (_lock)
                {
                    if (_clearActive)
                    {
                        ReleaseChainLocked(_head);
                        _head = _tail = null;
                        _clearActive = false;
                    }
                    _dispatching = false;
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var node in _nodes.Values) node.Expired = true;
                foreach (var node in _actionDict.Values) node.Expired = true;
                foreach (var node in _funcDict.Values) node.Expired = true;
                _nodes.Clear();
                _actionDict.Clear();
                _funcDict.Clear();
                ReleaseChainLocked(_pendingHead);
                _pendingHead = _pendingTail = null;
                if (_dispatching)
                {
                    _clearActive = true;
                    return;
                }
                ReleaseChainLocked(_head);
                _head = _tail = null;
            }
        }

        private Node RentLocked() => _pool.Count == 0 ? new Node() : _pool.Pop();

        private void AppendPendingLocked(Node node)
        {
            node.Prev = _pendingTail;
            if (_pendingTail == null) _pendingHead = node;
            else _pendingTail.Next = node;
            _pendingTail = node;
        }

        private void Retire(Node node)
        {
            if (node.Prev == null) _head = node.Next;
            else node.Prev.Next = node.Next;
            if (node.Next == null) _tail = node.Prev;
            else node.Next.Prev = node.Prev;

            lock (_lock)
            {
                if (node.Target != null && _nodes.TryGetValue(node.Target, out var current)
                    && ReferenceEquals(current, node)) _nodes.Remove(node.Target);
                if (node.Action != null && _actionDict.TryGetValue(node.Action, out current)
                    && ReferenceEquals(current, node)) _actionDict.Remove(node.Action);
                if (node.Func != null && _funcDict.TryGetValue(node.Func, out current)
                    && ReferenceEquals(current, node)) _funcDict.Remove(node.Func);
                ReturnLocked(node);
            }
        }

        private void ReleaseChainLocked(Node node)
        {
            while (node != null)
            {
                var next = node.Next;
                ReturnLocked(node);
                node = next;
            }
        }

        private void ReturnLocked(Node node)
        {
            node.Target = null;
            node.Action = null;
            node.Func = null;
            node.Prev = node.Next = null;
            node.Loop = false;
            node.Expired = false;
            if (_pool.Count < MaxPooledNodes) _pool.Push(node);
        }

        private sealed class Node
        {
            public ITickerNode Target;
            public Action Action;
            public Func<bool> Func;
            public Node Prev, Next;
            public bool Loop;
            public volatile bool Expired;

            public bool Invoke()
            {
                if (Target != null) return Target.OnTick();
                if (Func != null) return Func();
                Action();
                return Loop;
            }
        }

        private sealed class NodeComparer : IEqualityComparer<ITickerNode>
        {
            public static readonly NodeComparer Instance = new();
            public bool Equals(ITickerNode x, ITickerNode y) => ReferenceEquals(x, y);
            public int GetHashCode(ITickerNode obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
