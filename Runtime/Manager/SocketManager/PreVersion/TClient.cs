// /*----------------------------------------------------------------
// // author: Cookie(mcx)
// // date: 2024/7/5
// // describe: 对TcpClient类简单封装
// //----------------------------------------------------------------*/
//
// using System;
// using System.Net.Sockets;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace EasyFramework.Network
// {
//     public class TClient : IDisposable
//     {
//         public event Action<ESocketError> ErrorAction;
//         public bool Connected => _tcpClient is { Connected: true };
//         public string IP { get; private set; }
//         public int Port { get; private set; }
//         
//         private readonly TcpClient _tcpClient = new TcpClient();
//         private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
//         private readonly ReceiveBuffer _receiveBuffer = new ReceiveBuffer();
//         private readonly IPacketParser _packetParser;
//         private readonly object _writeLock = new object();
//         private NetworkStream _networkStream;
//
//         public TClient(IPacketParser packetParser)
//         {
//             _packetParser = packetParser;
//         }
//
//         public async Task<bool> Connect(string ip, int port)
//         {
//             IP = ip;
//             Port = port;
//             try
//             {
//                 await _tcpClient.ConnectAsync(ip, port);
//                 _networkStream = _tcpClient.GetStream();
//                 StartReceiving();
//
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Log.Warning($"Connect: {ex}");
//             }
//             return false;
//         }
//         
//         public void Send(byte[] data)
//         {
//             if (_networkStream == null)
//             {
//                 Log.Warning("Send: Not connected to server.");
//                 // ErrorAction?.Invoke(ESocketError.SendError);
//                 return;
//             }
//             try
//             {
//                 lock (_writeLock)
//                 {
//                     _networkStream.Write(data, 0, data.Length);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Log.Warning($"Send: {ex}");
//                 Disconnect();
//                 ErrorAction?.Invoke(ESocketError.SendError);
//             }
//         }
//
//         public async Task SendAsync(byte[] data)
//         {
//             if (_networkStream == null)
//             {
//                 Log.Warning("SendAsync: Not connected to server.");
//                 // ErrorAction?.Invoke(ESocketError.SendError);
//                 return;
//             }
//             try
//             {
//                 await Task.Run(() =>
//                 {
//                     lock (_writeLock)
//                     {
//                         // _networkStream.WriteAsync(data, 0, data.Length).Wait();
//                         _networkStream.Write(data, 0, data.Length);
//                     }
//                 });
//             }
//             catch (Exception ex)
//             {
//                 Log.Warning($"SendAsync: {ex}");
//                 Disconnect();
//                 ErrorAction?.Invoke(ESocketError.SendError);
//             }
//         }
//
//         private void StartReceiving()
//         {
//             // Task.Run(async () =>
//             Task.Run(() =>
//             {
//                 while (!_cancellationTokenSource.Token.IsCancellationRequested)
//                 {
//                     try
//                     {
//                         int size = _receiveBuffer.ChunkSize - _receiveBuffer.ChunkLastIndex;
//                         // int bytesRead = await _networkStream.ReadAsync(_receiveBuffer.Last, _receiveBuffer.LastIndex, size, _cancellationTokenSource.Token);
//                         int bytesRead = _networkStream.Read(_receiveBuffer.ChunkBuff, _receiveBuffer.ChunkLastIndex, size);
//                         if (bytesRead > 0)
//                         {
//                             _receiveBuffer.ChunkLastIndex += bytesRead;
//                             _packetParser.OnReceived(_receiveBuffer);
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Warning($"Receive: {ex}");
//                         Disconnect();
//                         F.MainThreadTask.Post(() =>
//                         {
//                             ErrorAction?.Invoke(ESocketError.ReceiveError);
//                         });
//                         
//                     }
//                 }
//             }, _cancellationTokenSource.Token);
//         }
//
//         public void Disconnect()
//         {
//             _cancellationTokenSource.Cancel();
//             _networkStream?.Close();
//             _tcpClient?.Close();
//             _networkStream = null;
//         }
//
//         public void Dispose()
//         {
//             Disconnect();
//             _cancellationTokenSource.Dispose();
//         }
//
//     }
// }