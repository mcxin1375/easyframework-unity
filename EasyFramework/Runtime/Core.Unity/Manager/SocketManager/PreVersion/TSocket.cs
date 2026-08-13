// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date: 2023/7/18
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.Net.Sockets;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace EasyFramework.Network
// {
//     public class TSocket : IDisposable
//     {
//         public event Action<TSocket, ESocketError> ErrorAction;
//         public bool Connected => _socket is { Connected: true };
//         public string IP { get; private set; }
//         public int Port { get; private set; }
//
//         private Socket _socket;
//         private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
//         private readonly ReceiveBuffer _receiveBuffer = new ReceiveBuffer();
//         private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
//         private readonly ITSocketParser _packetParser;
//         
//         private readonly object _lockObject = new object();
//         private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
//         
//         private bool _isSending;
//
//         public TSocket(ITSocketParser packetParser)
//         {
//             _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//             _socket.NoDelay = true;
//
//             _sendArgs.Completed += OnSendComplete;
//             _packetParser = packetParser;
//             _isSending = false;
//         }
//
//         public async Task<bool> Connect(string ip, int port)
//         {
//             IP = ip;
//             Port = port;
//             try
//             {
//                 // IPAddress[] addresses = Dns.GetHostAddresses(ip);
//                 // IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], port);
//                 // _sendArgs.RemoteEndPoint = ipEndPoint;
//                 // await _socket.ConnectAsync(ipEndPoint);
//                 
//                 await _socket.ConnectAsync(ip, port);
//                 StartReceiving();
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Log.Warning($"Connect: {ex}");
//             }
//             return false;
//         }
//
//         public void Send(byte[] bytes)
//         {
//             if (_socket == null || !_socket.Connected) return;
//
//             lock (_lockObject)
//             {
//                 _sendQueue.Enqueue(bytes);
//                 StartSend();
//             }
//         }
//         
//         private void StartReceiving()
//         {
//             Task.Run(async () =>
//             {
//                 while (!_cancellationTokenSource.Token.IsCancellationRequested)
//                 {
//                     try
//                     {
//                         if (_socket == null || !_socket.Connected) break;
//                     
//                         int size = _receiveBuffer.ChunkSize - _receiveBuffer.ChunkLastIndex;
//                         int len = _socket.Receive(_receiveBuffer.ChunkBuff, _receiveBuffer.ChunkLastIndex, size, SocketFlags.None, out SocketError errorCode);
//                         if (len > 0)
//                         {
//                             // Log.Info("StartReceiving", len);
//                             _receiveBuffer.ChunkLastIndex += len;
//                             _packetParser.OnReceived(this, _receiveBuffer);
//                         }
//
//                         if (errorCode != SocketError.Success)
//                         {
//                             Disconnect();
//                             ErrorAction?.Invoke(this, ESocketError.ReceiveError);
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Warning(ex);
//                         Disconnect();
//                         ErrorAction?.Invoke(this, ESocketError.ReceiveError);
//                     }
//
//                     await F.EasyTask.WaitFixedTime();
//                 }
//                 
//             }, _cancellationTokenSource.Token);
//         }
//         
//         public void Disconnect()
//         {
//             _cancellationTokenSource.Cancel();
//             
//             try
//             {
//                 if (_socket != null)
//                 {
//                     if (_socket.Connected)
//                     {
//                         _socket.Shutdown(SocketShutdown.Both);
//                     }
//                     _socket.Close();
//                     _socket = null;
//                 }
//                 
//                 lock (_lockObject)
//                 {
//                     _sendArgs?.Dispose();
//                     _sendArgs = null;
//                     _isSending = false;
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
//             _cancellationTokenSource.Dispose();
//         }
//
//         private void OnSendComplete(object sender, SocketAsyncEventArgs e)
//         {
//             switch (e.LastOperation)
//             {
//                 case SocketAsyncOperation.Connect:
//                     //MainThreadManager.Instance.Post(this.OnConnectComplete, e);
//                     // PostToMainThread(this.OnConnectComplete, e);
//                     // OnConnectComplete(e);
//                     break;
//                 case SocketAsyncOperation.Receive:
//                     //MainThreadManager.Instance.Post(this.OnRecvComplete, e);
//                     // PostToMainThread(this.OnRecvComplete, e);
//                     // OnRecvComplete(e);
//                     break;
//                 case SocketAsyncOperation.Send:
//                     //MainThreadManager.Instance.Post(this.OnSendComplete, e);
//                     // PostToMainThread(this.OnSendComplete, e);
//                     OnSendComplete(e);
//                     break;
//                 case SocketAsyncOperation.Disconnect:
//                     //MainThreadManager.Instance.Post(this.OnDisconnectComplete, e);
//                     // PostToMainThread(this.OnDisconnectComplete, e);
//                     // OnDisconnectComplete(e);
//                     break;
//                 default:
//                     throw new Exception("socket error: " + e.LastOperation);
//             }
//         }
//
//         private void OnSendComplete(SocketAsyncEventArgs e)
//         {
//             lock (_lockObject)
//             {
//                 if (e.SocketError != SocketError.Success)
//                 {
//                     Disconnect();
//                     ErrorAction?.Invoke(this, ESocketError.SendError);
//                     return;
//                 }
//                 _isSending = false;
//                 
//                 StartSend();
//             }
//         }
//
//         private void StartSend()
//         {
//             if (_socket == null || !_socket.Connected)
//             {
//                 ErrorAction?.Invoke(this, ESocketError.SendError);
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
//         }
//     }
// }