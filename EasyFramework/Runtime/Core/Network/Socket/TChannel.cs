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
// using System.Threading.Tasks;
//
// namespace EasyFramework
// {
//     public interface ISocketReceiverEx
//     {
//         void OnSocketError(ESocketOperation operation, SocketError socketError);
//         void OnReceived(IReceiveBuffer receiveBuffer);
//     }
//
//     public interface ISocketSender
//     {
//         void Send(byte[] bytes);
//     }
//     
//     public class TChannel : ISocketSender
//     {
//         // public ESocketState State { get; private set; } = ESocketState.Disconnected;
//         public string HostAddresses { get; private set; }
//         public int Port { get; private set; }
//         public bool IsConnected { get; private set; }
//         
//         private readonly SocketAsyncEventArgs _receiveArgs = new ();
//         private readonly SocketAsyncEventArgs _sendArgs = new ();
//         private readonly ReceiveBuffer _receiveBuffer = new ();
//         private readonly Queue<byte[]> _sendQueue = new ();
//         private ISocketReceiverEx _receiver;
//         private Socket _socket;
//
//         private readonly object _locker = new();
//         private bool _isSending;
//         private bool _isReceiving;
//         private bool _waitConnecting;
//         
//         public TChannel()
//         {
//             // _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//             // _socket.NoDelay = true;
//             _receiveArgs.Completed += OnReceiveComplete;
//             _sendArgs.Completed += OnSendComplete;
//         }
//
//         public async EasyTask<bool> ConnectAsync(string hostAddresses, int port, ISocketReceiverEx receiver)
//         {
//             HostAddresses = hostAddresses;
//             Port = port;
//             _receiver = receiver;
//             
//             IPAddress[] addresses = await Dns.GetHostAddressesAsync(hostAddresses);
//             if (addresses.Length == 0) return false;
//             
//             _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//             _socket.NoDelay = true;
//             _receiveArgs.AcceptSocket = _socket;
//             _sendArgs.AcceptSocket = _socket;
//             _sendArgs.RemoteEndPoint = new IPEndPoint(addresses[0], port);
//
//             _waitConnecting = true;
//             if (!_socket.ConnectAsync(_sendArgs))
//             {
//                 OnConnectComplete(_sendArgs);
//                 return IsConnected;
//             }
//
//             await EasyTask.WaitUntil(() => !_waitConnecting);
//             return IsConnected;
//         }
//         
//         public void Disconnect()
//         {
//             // State = ESocketState.Disconnected;
//             IsConnected = false;
//             HostAddresses = string.Empty;
//             Port = 0;
//             
//             _waitConnecting = false;
//             _isSending = false;
//             _isReceiving = false;
//             _receiveArgs.AcceptSocket = null;
//             _sendArgs.AcceptSocket = null;
//             _sendQueue.Clear();
//             _receiveBuffer.Reset();
//             _receiveArgs.Dispose();
//             _sendArgs.Dispose();
//
//             try
//             {
//                 if (_socket != null)
//                 {
//                     _socket.Disconnect(false);
//                     _socket.Dispose();
//                     _socket = null;
//                 }
//             }
//             catch (Exception e)
//             {
//                 FDebug.LogWarning(e.ToString(), LogTag.EasyFramework);
//             }
//         }
//
//         public void Send(byte[] bytes)
//         {
//             // if (State != ESocketState.Connected) return;
//             lock (_locker)
//             {
//                 _sendQueue.Enqueue(bytes);
//                 if (!_isSending) StartSend();
//             }
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
//                     OnReceiveComplete(e);
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
//                     OnReceiveComplete(e);
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
//         private void OnConnectComplete(SocketAsyncEventArgs e)
//         {
//             IsConnected = e.SocketError == SocketError.Success;
//             
//             if (IsConnected)
//             {
//                 StartReceiving();
//             }
//             else
//             {
//                 FDebug.LogWarning($"[{HostAddresses}:{Port}] OnConnectComplete - {e.SocketError.ToString()}");
//                 _receiver.OnSocketError(ESocketOperation.Connect, e.SocketError);
//             }
//             _waitConnecting = false;
//         }
//         private void OnDisconnectComplete(SocketAsyncEventArgs e)
//         {
//             IsConnected = false;
//             
//             if (e.SocketError != SocketError.Success)
//             {
//                 FDebug.LogWarning($"[{HostAddresses}:{Port}] OnDisconnectComplete - {e.SocketError.ToString()}");
//                 _receiver.OnSocketError(ESocketOperation.Disconnect, e.SocketError);
//             }
//         }
//         private void OnReceiveComplete(SocketAsyncEventArgs e)
//         {
//             if (e.SocketError != SocketError.Success)
//             {
//                 FDebug.LogWarning($"[{HostAddresses}:{Port}] OnReceiveComplete - {e.SocketError.ToString()}");
//                 _receiver.OnSocketError(ESocketOperation.Receive, e.SocketError);
//                 return;
//             }
//
//             if (e.BytesTransferred > 0)
//             {
//                 // Log.Info($"OnReceiveComplete BytesTransferred: {e.BytesTransferred}");
//                 _receiveBuffer.ChunkLastIndex += e.BytesTransferred;
//                 _receiver.OnReceived(_receiveBuffer);
//             }
//
//             _isReceiving = false;
//             StartReceiving();
//         }
//         private void OnSendComplete(SocketAsyncEventArgs e)
//         {
//             if (e.SocketError != SocketError.Success)
//             {
//                 FDebug.LogWarning($"[{HostAddresses}:{Port}] OnSendComplete - {e.SocketError.ToString()}");
//                 _receiver.OnSocketError(ESocketOperation.Send, e.SocketError);
//                 return;
//             }
//
//             lock (_locker)
//             {
//                 _isSending = false;
//             }
//             StartSend();
//         }
//
//         private void StartReceiving()
//         {
//             if (_isReceiving) return;
//             _isReceiving = true;
//
//             int size = _receiveBuffer.ChunkSize - _receiveBuffer.ChunkLastIndex;
//             ReceiveAsync(_receiveBuffer.ChunkBuff, _receiveBuffer.ChunkLastIndex, size);
//         }
//
//         private void ReceiveAsync(byte[] buffer, int offset, int count)
//         {
//             _receiveArgs.SetBuffer(buffer, offset, count);
//             if (_socket.ReceiveAsync(_receiveArgs)) return;
//             OnReceiveComplete(_receiveArgs);
//         }
//
//         private void StartSend()
//         {
//             lock (_locker)
//             {
//                 if (_isSending) return;
//                 if (_sendQueue.Count == 0) return;
//
//                 _isSending = true;
//                 var bytes = _sendQueue.Dequeue();
//
//                 _sendArgs.SetBuffer(bytes, 0, bytes.Length);
//                 if (!_socket.SendAsync(_sendArgs)) OnSendComplete(_sendArgs);
//             }
//         }
//     }
// }