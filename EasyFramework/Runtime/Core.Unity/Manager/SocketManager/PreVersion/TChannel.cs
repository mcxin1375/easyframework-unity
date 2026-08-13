// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date: 2023/7/18
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.Sockets;
//
// namespace EasyFramework.Network
// {
//     public class TChannel : IDisposable
//     {
//         public event Action<ESocketError> ErrorAction; 
//         public bool Connected => _socket is { Connected: true };
//         public string IP { get; private set; }
//         public int Port { get; private set; }
//         
//         private Socket _socket;
//
//         private SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
//         private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
//         private readonly ReceiveBuffer _receiveBuffer = new ReceiveBuffer();
//         private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
//         private readonly IPacketParser _packetParser;
//         
//         private bool _isSending;
//         private bool _isReceiving;
//         private Action<bool> _connectedAction;
//
//         public TChannel(IPacketParser packetParser)
//         {
//             _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//             _socket.NoDelay = true;
//             _receiveArgs.Completed += OnReceiveComplete;
//             _sendArgs.Completed += OnSendComplete;
//             _packetParser = packetParser;
//             _isSending = false;
//             _isReceiving = false;
//         }
//
//         public void Connect(string ip, int port, Action<bool> action)
//         {
//             _connectedAction = action;
//             IP = ip;
//             Port = port;
//             
//             IPAddress[] addresses = Dns.GetHostAddresses(ip);
//             IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], port);
//             _sendArgs.RemoteEndPoint = ipEndPoint;
//             if (!_socket.ConnectAsync(_sendArgs)) OnConnectComplete(_sendArgs);
//         }
//
//         public void Send(byte[] bytes)
//         {
//             if (_socket == null || !_socket.Connected) return;
//             
//             _sendQueue.Enqueue(bytes);
//             StartSend();
//         }
//         
//         public void Disconnect()
//         {
//             try
//             {
//                 _receiveArgs?.Dispose();
//                 _sendArgs?.Dispose();
//                 _sendArgs = null;
//                 _isSending = false;
//                 _isReceiving = false;
//
//                 if (_socket != null)
//                 {
//                     if (_socket.Connected)
//                     {
//                         _socket.Shutdown(SocketShutdown.Both);
//                     }
//                     _socket.Close();
//                     _socket = null;
//                 }
//             }
//             catch (Exception e)
//             {
//                 Log.Warning(e);
//             }
//         }
//
//         public void Dispose()
//         {
//             Disconnect();
//         }
//
//         private void OnReceiveComplete(object sender, SocketAsyncEventArgs e)
//         {
//             switch (e.LastOperation)
//             {
//                 case SocketAsyncOperation.Connect:
//                     OnConnectComplete(e);
//                     break;
//                 case SocketAsyncOperation.Receive:
//                     OnRecvComplete(e);
//                     break;
//                 case SocketAsyncOperation.Send:
//                     OnSendComplete(e);
//                     break;
//                 case SocketAsyncOperation.Disconnect:
//                     OnDisconnectComplete(e);
//                     break;
//                 default:
//                     throw new Exception("socket error: " + e.LastOperation);
//             }
//         }
//
//         private void OnSendComplete(object sender, SocketAsyncEventArgs e)
//         {
//             switch (e.LastOperation)
//             {
//                 case SocketAsyncOperation.Connect:
//                     OnConnectComplete(e);
//                     break;
//                 case SocketAsyncOperation.Receive:
//                     OnRecvComplete(e);
//                     break;
//                 case SocketAsyncOperation.Send:
//                     OnSendComplete(e);
//                     break;
//                 case SocketAsyncOperation.Disconnect:
//                     OnDisconnectComplete(e);
//                     break;
//                 default:
//                     throw new Exception("socket error: " + e.LastOperation);
//             }
//         }
//
//
//         private void OnConnectComplete(SocketAsyncEventArgs e)
//         {
//             F.MainThreadTask.Post(() =>
//             {
//                 var tmp = _connectedAction;
//                 _connectedAction = null;
//                 tmp?.Invoke(e.SocketError == SocketError.Success);
//                 if (e.SocketError == SocketError.Success)
//                 {
//                     StartReceiving();
//                 }
//             });
//         }
//         
//         private void OnDisconnectComplete(SocketAsyncEventArgs e)
//         {
//         }
//
//         private void OnRecvComplete(SocketAsyncEventArgs e)
//         {
//             if (e.SocketError != SocketError.Success)
//             {
//                 F.MainThreadTask.Post(() =>
//                 {
//                     ErrorAction?.Invoke(ESocketError.ReceiveError);
//                 });
//                 return;
//             }
//
//             if (e.BytesTransferred > 0)
//             {
//                 _receiveBuffer.ChunkLastIndex += e.BytesTransferred;
//                 _packetParser.OnReceived(_receiveBuffer);
//             }
//
//             _isReceiving = false;
//             StartReceiving();
//         }
//
//         private void OnSendComplete(SocketAsyncEventArgs e)
//         {
//             if (e.SocketError != SocketError.Success)
//             {
//                 F.MainThreadTask.Post(() =>
//                 {
//                     ErrorAction?.Invoke(ESocketError.SendError);
//                 });
//                 return;
//             }
//             _isSending = false;
//             StartSend();
//         }
//
//         private void StartReceiving()
//         {
//             if (_socket == null || !_socket.Connected)
//             {
//                 F.MainThreadTask.Post(() =>
//                 {
//                     ErrorAction?.Invoke(ESocketError.ReceiveError);
//                 });
//                 return;
//             }
//             if (_isReceiving) return;
//             _isReceiving = true;
//
//             int size = _receiveBuffer.ChunkSize - _receiveBuffer.ChunkLastIndex;
//             RecvAsync(_receiveBuffer.ChunkBuff, _receiveBuffer.ChunkLastIndex, size);
//         }
//
//         private void RecvAsync(byte[] buffer, int offset, int count)
//         {
//             _receiveArgs.SetBuffer(buffer, offset, count);
//             if (_socket.ReceiveAsync(_receiveArgs)) return;
//             OnRecvComplete(_receiveArgs);
//         }
//
//         private void StartSend()
//         {
//             if (_socket == null || !_socket.Connected)
//             {
//                 F.MainThreadTask.Post(() =>
//                 {
//                     ErrorAction?.Invoke(ESocketError.SendError);
//                 });
//                 return;
//             }
//
//             if (_isSending) return;
//             if (_sendQueue.Count == 0) return;
//
//             _isSending = true;
//             var bytes = _sendQueue.Dequeue();
//             
//             _sendArgs.SetBuffer(bytes, 0, bytes.Length);
//             if (!_socket.SendAsync(_sendArgs)) OnSendComplete(_sendArgs);
//
//         }
//     }
// }