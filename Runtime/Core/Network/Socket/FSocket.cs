// /*----------------------------------------------------------------
// // author: Cookie(mcx)
// // date: 2024/7/5
// // describe: 
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Concurrent;
// using System.IO;
// using System.Net;
// using System.Net.Sockets;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace EasyFramework
// {
//     public class FSocket
//     {
//         public event Action<ESocketState> OnStateChanged;
//         
//         public int SendMillisecondsDelay { get; set; } = 10;
//         public bool IsConnected { get; private set; }
//         public string Host { get; private set; }
//         public int Port { get; private set; }
//
//         private ESocketState _state;
//         public ESocketState State
//         {
//             get => _state;
//             set
//             {
//                 if (_state == value) return;
//                 _state = value;
//
//                 switch (_state)
//                 {
//                     case ESocketState.ReceiveError:
//                     case ESocketState.SendError:
//                         
//                         if (IsConnected) Disconnect();
//                         
//                         break;
//                 }
//                 
//                 F.MainThreadSystem.Post(() =>
//                 {
//                     OnStateChanged?.Invoke(_state);
//                 });
//             }
//         }
//
//         private Socket _socket;
//         private const int BufferSize = 1024;
//         private readonly ConcurrentQueue<byte[]> _sendQueue = new ();
//         private readonly ReceiveBuffer _receiveBuffer = new ReceiveBuffer();
//         private readonly IMessageParser _messageParser;
//         private readonly byte[] _buffer = new byte[BufferSize];
//         
//         public FSocket(IMessageParser messageParser)
//         {
//             _messageParser = messageParser;
//         }
//
//         public async Task<bool> ConnectAsync(string host, int port)
//         {
//             try
//             {
//                 Host = host;
//                 Port = port;
//                 
//                 IPAddress ipAddress = IPAddress.Parse(host);
//                 IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);
//                 _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//
//                 await _socket.ConnectAsync(remoteEndPoint);
//                 IsConnected = true;
//                 // _cancellationTokenSource = new CancellationTokenSource();
//                 StartSend(); // 启动异步发送数据
//                 StartReceive(); // 启动异步接收数据
//                 
//                 State = ESocketState.Connected;
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 // Console.WriteLine($"Failed to connect: {ex.Message}");
//                 Log.Error(ex);
//                 State = ESocketState.Disconnected;
//                 return false;
//             }
//         }
//         
//         public void Disconnect()
//         {
//             try
//             {
//                 if (IsConnected)
//                 {
//                     IsConnected = false;
//                     _sendQueue.Clear();
//                     if (_socket != null)
//                     {
//                         _socket.Shutdown(SocketShutdown.Both);
//                         _socket.Close();
//                         _socket = null;
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Log.Warning(ex);
//             }
//         }
//         
//         public void Send(byte[] data)
//         {
//             if (!IsConnected) return;
//
//             // 将数据加入队列
//             _sendQueue.Enqueue(data);
//         }
//
//         private void StartSend()
//         {
//             Task.Run(async () =>
//             {
//                 try
//                 {
//                     while (IsConnected)
//                     {
//                         while (_sendQueue.TryDequeue(out var message))
//                         {
//                             // await _sendSemaphore.WaitAsync(); // 确保顺序发送
//                             // await _socket.SendAsync(message, SocketFlags.None, _cancellationTokenSource.Token);
//                             await _socket.SendAsync(message, SocketFlags.None);
//                         }
//
//                         await Task.Delay(SendMillisecondsDelay);
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     State = ESocketState.SendError;
//                 }
//             });
//         }
//
//         private void StartReceive()
//         {
//             Task.Run(async () =>
//             {
//                 try
//                 {
//                     while (IsConnected)
//                     {
//                         // int receivedBytes = await _socket.ReceiveAsync(_buffer, SocketFlags.None, _cancellationTokenSource.Token);
//                         int receivedBytes = await _socket.ReceiveAsync(_buffer, SocketFlags.None);
//                         if (receivedBytes > 0)
//                         {
//                             _receiveBuffer.Write(_buffer, 0, receivedBytes);
//                             _messageParser.OnReceive(_receiveBuffer);
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     State = ESocketState.ReceiveError;
//                 }
//             });
//         }
//     }
// }