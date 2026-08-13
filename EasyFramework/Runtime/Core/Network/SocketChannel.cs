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
        
        private int _sendState;
        private ETaskState _taskState = ETaskState.None;
        
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
            Host = host;
            Port = port;
            
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(Host);
            if (addresses.Length == 0) return false;
            
            _sendArgs.RemoteEndPoint = new IPEndPoint(addresses[0], port);

            _taskState = ETaskState.Connecting;
            if (!_socket.ConnectAsync(_sendArgs)) OnConnectComplete(_sendArgs);
            
            await ETask.WaitUntil(() => _taskState == ETaskState.None);
            return IsConnected;
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            IsConnected = false;
            Volatile.Write(ref _sendState, 0);
            _sendBuffer.Clear();
            _receiveBuffer.Clear();
            
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
        }
        public async ETask<bool> DisconnectAsync()
        {
            if (!IsConnected) return false;

            IsConnected = false;
            Volatile.Write(ref _sendState, 0);
            _sendBuffer.Clear();
            _receiveBuffer.Clear();
            
            _taskState = ETaskState.Disconnecting;
            if (!_socket.DisconnectAsync(_sendArgs)) OnDisconnectComplete(_sendArgs);
            
            await ETask.WaitUntil(() => _taskState == ETaskState.None);
            return IsConnected;
        }

        public void Dispose()
        {
            Disconnect();
            
            _sendArgs.Dispose();
            _receiveArgs.Dispose();
            _sendBuffer.Dispose();
            _receiveBuffer.Dispose();
        }

        public void Send(byte[] buffer) => Send(buffer.AsSpan());
        public void Send(byte[] buffer, int offset, int count) => Send(new ReadOnlySpan<byte>(buffer, offset, count));
        public void Send(ReadOnlySpan<byte> span)
        {
            if (!IsConnected) return;
            
            _sendBuffer.Write(span);
            StartSending();
        }

        public void Send()
        {
            if (!IsConnected) return;
            StartSending();
        }

        private void OnSocketEvent(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    OnConnectComplete(e);
                    break;
                case SocketAsyncOperation.Receive:
                    OnReceiveComplete(e);
                    break;
                case SocketAsyncOperation.Send:
                    OnSendComplete(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    OnDisconnectComplete(e);
                    break;
                default:
                    throw new Exception("socket error: " + e.LastOperation);
            }
        }
        private void OnConnectComplete(SocketAsyncEventArgs e)
        {
            IsConnected = e.SocketError == SocketError.Success;

            if (e.SocketError != SocketError.Success)
            {
                Disconnect();
                _handler.OnSocketError(ESocketOperation.Connect, e.SocketError);
            }

            if (IsConnected) StartReceiving();
            
            _taskState = ETaskState.None;
        }
        private void OnDisconnectComplete(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Disconnect();
                _handler.OnSocketError(ESocketOperation.Disconnect, e.SocketError);
            }
            
            _taskState = ETaskState.None;
        }
        private void OnReceiveComplete(SocketAsyncEventArgs e)
        {
            // 对端关闭
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                if (IsConnected)
                {
                    Disconnect();
                    _handler.OnSocketError(ESocketOperation.Receive, e.SocketError);
                }
                return;
            }
            
            _receiveBuffer.Advance(e.BytesTransferred);
            _handler.OnReceive(_receiveBuffer);
            
            StartReceiving();
        }
        private void OnSendComplete(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                if (IsConnected)
                {
                    Disconnect();
                    _handler.OnSocketError(ESocketOperation.Send, e.SocketError);
                }
                return;
            }
            
            _sendBuffer.AdvanceRead(e.BytesTransferred);
            
            Volatile.Write(ref _sendState, 0);
            StartSending();
        }

        private void StartReceiving()
        {
            var segment = _receiveBuffer.GetSegment(ChunkSize);
            _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
            if (!_socket.ReceiveAsync(_receiveArgs)) OnReceiveComplete(_receiveArgs);
        }
        
        private void StartSending()
        {
            if (!IsConnected || _sendBuffer.Length == 0) return;
            
            if (Interlocked.CompareExchange(ref _sendState, 1, 0) != 0) return;
            
            if (!_sendBuffer.TryPeekSegment(ChunkSize, out var segment))
            {
                Volatile.Write(ref _sendState, 0);
                return;
            }
            _sendArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
            if (!_socket.SendAsync(_sendArgs)) OnSendComplete(_sendArgs);
        }

    }
}