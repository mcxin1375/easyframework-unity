// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date: 2023/7/18
// // describe:
// //----------------------------------------------------------------*/
//
//
//
// using System;
// using System.IO;
// using System.Net;
// using System.Net.Sockets;
//
//
// namespace EasyFramework.Network
// {
//     public class NTChannel : BaseChannel
//     {
//         private Socket _socket;
//
//         /// <summary>
//         /// �첽�����׽���
//         /// </summary>
//         private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
//         /// <summary>
//         /// �첽�����׽���
//         /// </summary>
//         private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
//
//         private readonly ReceiveBuffer _recvBuffer = new ReceiveBuffer();
//         //private readonly CircularBuffer sendBuffer = new CircularBuffer();
//         private readonly PacketParser _parser;
//         private readonly MemoryStream _memoryStream;
//
//         //private readonly byte[] packetSizeCache;
//         private bool _isSending;
//         private bool _isRecving;
//         private BasePacket _sendCache;
//
//
//         public NTChannel() : base(NetworkChannelType.TCP)
//         {
//             _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//             _socket.NoDelay = true;
//
//             _recvArgs.Completed += this.OnRecvComplete;
//             _sendArgs.Completed += this.OnSendComplete;
//
//             //packetSizeCache = new byte[Packet.PACKET_HEAD];
//             _memoryStream = new MemoryStream(ushort.MaxValue);
//             _parser = new PacketParser(_recvBuffer, _memoryStream);
//
//             _isSending = false;
//             _isRecving = false;
//         }
//
//         protected override void RequestConnect(IPEndPoint ipEndPoint)
//         {
//             _sendArgs.RemoteEndPoint = ipEndPoint;
//             if (_socket.ConnectAsync(_sendArgs))
//             {
//                 return;
//             }
//             OnConnectComplete(_sendArgs);
//         }
//
//         protected override void RequestDisconnect()
//         {
//             try
//             {
//                 _recvArgs.Dispose();
//                 _sendArgs.Dispose();
//                 _memoryStream.Dispose();
//                 _recvArgs = null;
//                 _sendArgs = null;
//                 _isSending = false;
//                 _isRecving = false;
//
//                 if (_socket != null)
//                 {
//                     if (_socket.Connected)
//                     {
//                         _socket.Shutdown(SocketShutdown.Both);
//                     }
//                     _socket.Close();
//                     _socket = null;
//                     //Log.Warning("------------- �����Ͽ� TChannel. Disconnected! ", RemoteAddress.ToString());
//                 }
//             }
//             catch (Exception e)
//             {
//                 Log.Warning("Error: ", e.ToString());
//             }
//         }
//
//         protected override void OnSend(BasePacket message)
//         {
//             StartSend();
//         }
//
//         private void OnRecvComplete(object sender, SocketAsyncEventArgs e)
//         {
//             //Log.Info("[TCP] TChannel OnInComplete: ", e.LastOperation);
//             if (_socket == null)
//             {
//                 Log.Warning("NTChannel OnRecvComplete. _socket is null!");
//                 return;
//             }
//
//             switch (e.LastOperation)
//             {
//                 case SocketAsyncOperation.Connect:
//                     //MainThreadManager.Instance.Post(this.OnConnectComplete, e);
//                     // PostToMainThread(OnConnectComplete, e);
//                     OnConnectComplete(e);
//                     break;
//                 case SocketAsyncOperation.Receive:
//                     //MainThreadManager.Instance.Post(this.OnRecvComplete, e);
//                     //Log.Info("Start PostToMainThread");
//                     // PostToMainThread(OnRecvComplete, e);
//                     OnRecvComplete(e);
//                     break;
//                 case SocketAsyncOperation.Send:
//                     //MainThreadManager.Instance.Post(this.OnSendComplete, e);
//                     // PostToMainThread(OnSendComplete, e);
//                     OnSendComplete(e);
//                     break;
//                 case SocketAsyncOperation.Disconnect:
//                     //MainThreadManager.Instance.Post(this.OnDisconnectComplete, e);
//                     // PostToMainThread(OnDisconnectComplete, e);
//                     OnDisconnectComplete(e);
//                     break;
//                 default:
//                     SocketError = e.SocketError;
//                     NetworkState = ENetworkState.RecvError;
//                     throw new Exception("socket error: " + e.LastOperation);
//             }
//         }
//
//         private void OnSendComplete(object sender, SocketAsyncEventArgs e)
//         {
//             //Log.Warning("TChannel OnSendComplete: ", e.LastOperation.ToString());
//             
//             if (_socket == null)
//             {
//                 Log.Warning("NTChannel OnSendComplete. _socket is null!");
//                 return;
//             }
//             
//             switch (e.LastOperation)
//             {
//                 case SocketAsyncOperation.Connect:
//                     //MainThreadManager.Instance.Post(this.OnConnectComplete, e);
//                     // PostToMainThread(this.OnConnectComplete, e);
//                     OnConnectComplete(e);
//                     break;
//                 case SocketAsyncOperation.Receive:
//                     //MainThreadManager.Instance.Post(this.OnRecvComplete, e);
//                     // PostToMainThread(this.OnRecvComplete, e);
//                     OnRecvComplete(e);
//                     break;
//                 case SocketAsyncOperation.Send:
//                     //MainThreadManager.Instance.Post(this.OnSendComplete, e);
//                     // PostToMainThread(this.OnSendComplete, e);
//                     OnSendComplete(e);
//                     break;
//                 case SocketAsyncOperation.Disconnect:
//                     //MainThreadManager.Instance.Post(this.OnDisconnectComplete, e);
//                     // PostToMainThread(this.OnDisconnectComplete, e);
//                     OnDisconnectComplete(e);
//                     break;
//                 default:
//                     SocketError = e.SocketError;
//                     NetworkState = ENetworkState.SendError;
//                     throw new Exception("socket error: " + e.LastOperation);
//             }
//         }
//
//
//         private void OnConnectComplete(SocketAsyncEventArgs e)
//         {
//             // Log.Info("OnConnectComplete");
//             if (_socket == null) return;
//
//             //SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
//
//             SocketError = e.SocketError;
//             NetworkState = e.SocketError != SocketError.Success ? ENetworkState.ConnectedError : ENetworkState.Connected;
//
//             if (NetworkState == ENetworkState.Connected)
//                 StartRecv();
//         }
//         
//         private void OnDisconnectComplete(SocketAsyncEventArgs e)
//         {
//             SocketError = e.SocketError;
//             NetworkState = ENetworkState.Disconnected;
//         }
//
//         private void OnRecvComplete(SocketAsyncEventArgs e)
//         {
//             //Log.Warning("[TCP]", "----------------  OnRecvComplete :", e.SocketError.ToString());
//
//             //SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
//
//             if (e.SocketError != SocketError.Success)
//             {
//                 SocketError = e.SocketError;
//                 NetworkState = ENetworkState.RecvError;
//                 return;
//             }
//
//             if (e.BytesTransferred > 0)
//             {
//                 _recvBuffer.LastIndex += e.BytesTransferred;
//                 if (_recvBuffer.LastIndex == _recvBuffer.ChunkSize)
//                 {
//                     _recvBuffer.AddLast();
//                     _recvBuffer.LastIndex = 0;
//                 }
//
//                 // �յ���Ϣ�ص�
//                 while (true)
//                 {
//                     byte[] headBytes, dataBytes;
//                     try
//                     {
//                         if (!_parser.Parse(out headBytes, out dataBytes))
//                         {
//                             break;
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         NetworkState = ENetworkState.RecvError;
//                         Log.Warning(ex);
//                         return;
//                     }
//
//                     OnReceive(headBytes, dataBytes);
//                 }
//             }
//
//             _isRecving = false;
//             StartRecv();
//         }
//
//         private void OnSendComplete(SocketAsyncEventArgs e)
//         {
//             //Log.Warning("[TCP]", "OnSendComplete :", e.SocketError.ToString());
//             //SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
//
//             if (e.SocketError != SocketError.Success)
//             {
//                 SocketError = e.SocketError;
//                 NetworkState = ENetworkState.SendError;
//                 return;
//             }
//             _sendCache = null;
//             _isSending = false;
//             StartSend();
//         }
//
//         // ��������
//         private void StartRecv()
//         {
//             //Log.Info("[TCP]   ---------------- StartRecv");
//             if (_socket == null || !_socket.Connected) return;
//             if (_isRecving) return;
//             _isRecving = true;
//
//             int size = _recvBuffer.ChunkSize - _recvBuffer.LastIndex;
//             RecvAsync(_recvBuffer.Last, _recvBuffer.LastIndex, size);
//         }
//
//         private void RecvAsync(byte[] buffer, int offset, int count)
//         {
//             _recvArgs.SetBuffer(buffer, offset, count);
//             if (_socket.ReceiveAsync(_recvArgs)) return;
//             OnRecvComplete(_recvArgs);
//         }
//
//         private void StartSend()
//         {
//             if (_socket == null || !_socket.Connected)
//             {
//                 return;
//             }
//             //if (NetworkState != ENetworkState.Connected)
//             //{
//             //    return;
//             //}
//
//             if (_isSending)
//                 return;
//
//             if (_sendCache == null)
//             {
//                 // û��������Ҫ����
//                 if (SendQueue.Count == 0)
//                 {
//                     _isSending = false;
//                     return;
//                 }
//                 _sendCache = SendQueue.Dequeue();
//             }
//             _isSending = true;
//
//             //Log.Warning("[TCP]", "Start SendAsync MsgId:", sendCache.MsgId);
//             SendAsync(_sendCache.packetBytes, 0, _sendCache.packetBytes.Length);
//         }
//
//         private void SendAsync(byte[] buffer, int offset, int count)
//         {
//             _sendArgs.SetBuffer(buffer, offset, count);
//             if (_socket.SendAsync(_sendArgs))
//             {
//                 return;
//             }
//             OnSendComplete(_sendArgs);
//
//             // ͬ������
//             //SocketError socketError;
//             //this.socket.Send(buffer, offset, count, SocketFlags.None, out socketError);
//
//             //this.OnSocketError(socketError);
//             //if (socketError != SocketError.Success)
//             //{
//             //    this.OnNetworkState(ENetworkState.SendError);
//             //}
//             //isSending = false;
//             //sendCache = null;
//         }
//
//
//
//     }
// }