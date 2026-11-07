/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/2
// describe:
//----------------------------------------------------------------*/

using System;
using System.Net.Sockets;

namespace EasyFramework
{
    public interface ISocketHandler
    {
        void OnSocketError(ESocketOperation operation, SocketError socketError);
        void OnReceive(IRingBuffer buffer);
    }

    public interface ISocketChannel : IDisposable
    {
        // EndPoint RemoteEndPoint { get; }
        string Host { get; }
        int Port { get; }
        bool IsConnected { get; }

        IRingBuffer SendBuffer { get; }

        // void Connect(string host, int port);
        ETask<bool> ConnectAsync(string host, int port);
        void Disconnect();
        // EasyTask DisconnectAsync();

        void Send();
        void Send(ReadOnlySpan<byte> span);
        void Send(byte[] bytes);
        void Send(byte[] bytes, int offset, int length);
    }
}