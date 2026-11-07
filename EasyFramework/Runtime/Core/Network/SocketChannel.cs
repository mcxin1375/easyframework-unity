/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/2
// describe:
//----------------------------------------------------------------*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EasyFramework
{
    public enum ESocketOperation
    {
        Connect,
        Send,
        Receive,
        Disconnect,
    }
    public class SocketChannel : ISocketChannel
    {
        public enum ETaskState
        {
            None = 0,
            Connecting = 1,
            Disconnecting = 2,
        }
        
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool IsConnected { get; private set; }

        public IRingBuffer SendBuffer => _sendBuffer;
        
        private readonly Socket _socket;
        private readonly SocketAsyncEventArgs _sendArgs = new ();
        private readonly SocketAsyncEventArgs _receiveArgs = new ();
        private readonly IRingBuffer _sendBuffer = new RingBuffer();
        private readonly IRingBuffer _receiveBuffer = new RingBuffer();
        private readonly ISocketHandler _handler;

        private const int ChunkSize = 16 * 1024;
        private readonly object _socketLock = new();
        private volatile bool _disposed;
        private int _pendingOperations;
        private bool _resourcesDisposed;
        
        private int _sendState;
        private volatile ETaskState _taskState = ETaskState.None;
        
        public SocketChannel(ISocketHandler handler)
        {
            _handler = handler;
            
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            _receiveArgs.AcceptSocket = _socket;
            _sendArgs.AcceptSocket = _socket;
            
            _receiveArgs.Completed += OnSocketEvent;
            _sendArgs.Completed += OnSocketEvent;
        }
        
        public async ETask<bool> ConnectAsync(string host, int port)
        {
            if (_disposed) return false;
            Host = host;
            Port = port;
            
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(Host);
            if (addresses.Length == 0) return false;
            
            bool pending;
            lock (_socketLock)
            {
                if (_disposed) return false;
                _sendArgs.RemoteEndPoint = new IPEndPoint(addresses[0], port);
                _taskState = ETaskState.Connecting;
                _pendingOperations++;
                try { pending = _socket.ConnectAsync(_sendArgs); }
                catch
                {
                    _pendingOperations--;
                    _taskState = ETaskState.None;
                    throw;
                }
            }
            if (!pending) OnSocketEvent(this, _sendArgs);
            
            await ETask.WaitUntil(() => _taskState == ETaskState.None);
            return IsConnected;
        }

        public void Disconnect()
        {
            lock (_socketLock)
            {
                if (_disposed || !IsConnected) return;
                IsConnected = false;
                Volatile.Write(ref _sendState, 0);
                _sendBuffer.Clear();
                _receiveBuffer.Clear();
                try { _socket.Shutdown(SocketShutdown.Both); }
                catch (SocketException e) { FDebug.LogException(e); }
                finally { _socket.Close(); }
            }
        }
        public async ETask<bool> DisconnectAsync()
        {
            bool pending;
            lock (_socketLock)
            {
                if (_disposed || !IsConnected) return false;
                IsConnected = false;
                Volatile.Write(ref _sendState, 0);
                _sendBuffer.Clear();
                _receiveBuffer.Clear();
                _taskState = ETaskState.Disconnecting;
                _pendingOperations++;
                try { pending = _socket.DisconnectAsync(_sendArgs); }
                catch
                {
                    _pendingOperations--;
                    _taskState = ETaskState.None;
                    throw;
                }
            }
            if (!pending) OnSocketEvent(this, _sendArgs);
            
            await ETask.WaitUntil(() => _taskState == ETaskState.None);
            return IsConnected;
        }

        public void Dispose()
        {
            lock (_socketLock)
            {
                if (_disposed) return;
                _disposed = true;
                IsConnected = false;
                _taskState = ETaskState.None;
                Volatile.Write(ref _sendState, 0);
                // Close the handle even when connection never succeeded. Pending callbacks
                // retain their args and pooled buffers until they have finished.
                try { _socket.Dispose(); }
                finally { ReleaseResources(); }
            }
        }

        // Called under _socketLock; each submitted operation releases its hold exactly once.
        private void ReleaseResources()
        {
            if (!_disposed || _pendingOperations != 0 || _resourcesDisposed) return;
            _resourcesDisposed = true;
            _sendArgs.Completed -= OnSocketEvent;
            _receiveArgs.Completed -= OnSocketEvent;
            _sendArgs.Dispose();
            _receiveArgs.Dispose();
            _sendBuffer.Dispose();
            _receiveBuffer.Dispose();
        }

        public void Send(byte[] buffer) => Send(buffer.AsSpan());
        public void Send(byte[] buffer, int offset, int count) => Send(new ReadOnlySpan<byte>(buffer, offset, count));
        public void Send(ReadOnlySpan<byte> span)
        {
            lock (_socketLock)
            {
                if (_disposed || !IsConnected) return;
                _sendBuffer.Write(span);
            }
            StartSending();
        }

        public void Send()
        {
            if (!IsConnected) return;
            StartSending();
        }

        private void OnSocketEvent(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (_disposed) return;
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Connect: OnConnectComplete(e); break;
                    case SocketAsyncOperation.Receive: OnReceiveComplete(e); break;
                    case SocketAsyncOperation.Send: OnSendComplete(e); break;
                    case SocketAsyncOperation.Disconnect: OnDisconnectComplete(e); break;
                    default: throw new Exception("socket error: " + e.LastOperation);
                }
            }
            finally
            {
                lock (_socketLock)
                {
                    _pendingOperations--;
                    ReleaseResources();
                }
            }
        }
        private void OnConnectComplete(SocketAsyncEventArgs e)
        {
            lock (_socketLock)
            {
                if (_disposed) return;
                IsConnected = e.SocketError == SocketError.Success;
            }
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    Disconnect();
                    if (!_disposed) _handler.OnSocketError(ESocketOperation.Connect, e.SocketError);
                }
                if (IsConnected) StartReceiving();
            }
            finally { _taskState = ETaskState.None; }
        }
        private void OnDisconnectComplete(SocketAsyncEventArgs e)
        {
            if (_disposed) return;
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    Disconnect();
                    if (!_disposed) _handler.OnSocketError(ESocketOperation.Disconnect, e.SocketError);
                }
            }
            finally { _taskState = ETaskState.None; }
        }
        private void OnReceiveComplete(SocketAsyncEventArgs e)
        {
            lock (_socketLock)
            {
                if (_disposed || !IsConnected) return;
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                    _receiveBuffer.Advance(e.BytesTransferred);
            }
            // 对端关闭
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                if (IsConnected)
                {
                    Disconnect();
                    if (!_disposed) _handler.OnSocketError(ESocketOperation.Receive, e.SocketError);
                }
                return;
            }
            
            if (!_disposed) _handler.OnReceive(_receiveBuffer);
            
            StartReceiving();
        }
        private void OnSendComplete(SocketAsyncEventArgs e)
        {
            lock (_socketLock)
            {
                if (_disposed || !IsConnected) return;
                if (e.SocketError == SocketError.Success)
                    _sendBuffer.AdvanceRead(e.BytesTransferred);
            }
            if (e.SocketError != SocketError.Success)
            {
                if (IsConnected)
                {
                    Disconnect();
                    if (!_disposed) _handler.OnSocketError(ESocketOperation.Send, e.SocketError);
                }
                return;
            }
            
            Volatile.Write(ref _sendState, 0);
            StartSending();
        }

        private void StartReceiving()
        {
            bool pending;
            lock (_socketLock)
            {
                if (_disposed || !IsConnected) return;
                var segment = _receiveBuffer.GetSegment(ChunkSize);
                _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
                _pendingOperations++;
                try { pending = _socket.ReceiveAsync(_receiveArgs); }
                catch { _pendingOperations--; throw; }
            }
            if (!pending) OnSocketEvent(this, _receiveArgs);
        }
        
        private void StartSending()
        {
            bool pending;
            lock (_socketLock)
            {
                if (_disposed || !IsConnected || _sendBuffer.Length == 0) return;
                if (Interlocked.CompareExchange(ref _sendState, 1, 0) != 0) return;
                if (!_sendBuffer.TryPeekSegment(ChunkSize, out var segment))
                {
                    Volatile.Write(ref _sendState, 0);
                    return;
                }
                _sendArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
                _pendingOperations++;
                try { pending = _socket.SendAsync(_sendArgs); }
                catch
                {
                    _pendingOperations--;
                    Volatile.Write(ref _sendState, 0);
                    throw;
                }
            }
            if (!pending) OnSocketEvent(this, _sendArgs);
        }

    }
}
