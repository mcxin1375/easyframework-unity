// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
//
// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.Sockets;
//
// namespace EasyFramework.Network
// {
//
//     public class NetworkClient
//     {
//         public event Action<ENetworkState> OnNetworkStateAction;
//         public event Action<SocketError> OnSocketErrorAction;
//         
//         public bool IsConnect => _channel?.IsConnect ?? false;
//         public ENetworkState NetworkState => _channel?.NetworkState ?? ENetworkState.Disconnected;
//
//         // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
//         private readonly Queue<Action> _queue = new Queue<Action>();
//         private readonly Queue<ReadPacket> _receiveQueue = new Queue<ReadPacket>();
//         
//         private readonly Action<BasePacket> _onReceiveAction;
//
//         private Action<bool> _connectAction = null;
//         private BaseChannel _channel;
//         private ENetworkState _networkState;
//         private SocketError _socketError;
//
//         public NetworkClient(Action<BasePacket> receiveAction)
//         {
//             _onReceiveAction = receiveAction;
//         }
//         
//         public void Connect(string address, Action<bool> action = null)
//         {
//             string[] arr = address.Split(':');
//             if (arr.Length != 2)
//             {
//                 action?.Invoke(false);
//                 return;
//             }
//
//             Connect(arr[0], int.Parse(arr[1]), action);
//         }
//
//         public void Connect(string ip, int port, Action<bool> action = null)
//         {
//             if (_channel is { IsConnect: true } && _channel.IPAddress.Address.ToString() == ip && _channel.IPAddress.Port == port)
//             {
//                 action?.Invoke(true);
//                 return;
//             }
//
//             _connectAction = action;
//
//             // 获取 DNS 主机地址
//             //  DNS 服务器中查询与某个主机名关联的 IP 地址。 如果 hostNameOrAddress 是 IP 地址，则不查询 DNS 服务器直接返回此地址
//             IPAddress[] addresses = null;
//             try
//             {
//                 //Log.Warning("---------------- GetHostAddresses: ", ip);
//                 addresses = Dns.GetHostAddresses(ip);
//             }
//             catch (System.Exception excep)
//             {
//                 Log.Warning("---------------- DNS.GetHostAddresses Error:", excep);
//                 action?.Invoke(false);
//                 return;
//             }
//
//             IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], port);
//
//             Disconnect();
//
//             _channel = new NTChannel();
//             _channel.OnReceiveAction += OnReceive;
//             // _channel.OnSocketErrorAction += OnError;
//             // _channel.OnNetworkStateAction += OnState;
//             Log.Info($"NetworkClient. Create NTChannel! address:{ip}:{port}");
//
//             _channel.Connect(ipEndPoint);
//
//             _networkState = _channel.NetworkState;
//             _socketError = _channel.SocketError;
//             F.Tick.Add(OnTick);
//         }
//
//         private void OnReceive(byte[] headBody, byte[] dataBody)
//         {
//             // _onReceiveAction?.Invoke(new ReadPacket(headBody, dataBody));
//             lock (_receiveQueue)
//             {
//                 _receiveQueue.Enqueue(new ReadPacket(headBody, dataBody));
//             }
//         }
//
//         // private void OnError(SocketError errorCode)
//         // {
//         //     Log.Warning("OnNetworkError. ", errorCode.ToString());
//         //     OnSocketErrorAction?.Invoke(errorCode);
//         // }
//         //
//         // private void OnState(ENetworkState eNetworkState)
//         // {
//         //     if (_connectAction != null)
//         //     {
//         //         _connectAction(IsConnect);
//         //         _connectAction = null;
//         //     }
//         //     
//         //     OnNetworkStateAction?.Invoke(eNetworkState);
//         // }
//
//         private bool OnTick()
//         {
//             if (_channel != null)
//             {
//                 lock (_receiveQueue)
//                 {
//                     while (_receiveQueue.Count > 0)
//                     {
//                         BasePacket packet = _receiveQueue.Dequeue();
//                         _onReceiveAction?.Invoke(packet);
//                     }
//                 }
//                 
//                 lock (_queue)
//                 {
//                     while (_queue.Count > 0)
//                     {
//                         _queue.Dequeue()?.Invoke();
//                     }
//                 }
//
//                 if (_networkState != _channel.NetworkState)
//                 {
//                     _networkState = _channel.NetworkState;
//                     OnNetworkStateAction?.Invoke(_networkState);
//                     
//                     if (_connectAction != null)
//                     {
//                         _connectAction(IsConnect);
//                         _connectAction = null;
//                     }
//                 }
//                 if (_socketError != _channel.SocketError)
//                 {
//                     _socketError = _channel.SocketError;
//                     OnSocketErrorAction?.Invoke(_socketError);
//                 }
//
//                 //lock (receiveQueue)
//                 //{
//                 //    while (receiveQueue.Count > 0)
//                 //    {
//                 //        onReceiveAction?.Invoke(receiveQueue.Dequeue());
//                 //    }
//                 //}
//
//             }
//             return true;
//         }
//
//         public void Disconnect()
//         {
//             F.Tick.Remove(OnTick);
//             _networkState = ENetworkState.Disconnected;
//             _socketError = SocketError.SocketError;
//             if (_channel != null)
//             {
//                 Log.Warning("NNetworkClient Disconnect: ", _channel.IPAddress.ToString());
//                 
//                 _channel.Disconnect();
//                 _channel = null;
//                 
//                 lock (_receiveQueue)
//                 {
//                     _receiveQueue.Clear();
//                 }
//                 //Log.Warning("NetworkClient Disconnect!", networkProtocol.ToString(), channel.IPAddress.ToString());
//                 lock (_queue)
//                 {
//                     _queue.Clear();
//                 }
//             }
//         }
//
//         public void Send(WritePacket packet)
//         {
//             if (_channel != null)
//             {
//                 _channel.Send(packet);
//             }
//         }
//
//         // private void PostToMainThread(Action<SocketAsyncEventArgs> callback, SocketAsyncEventArgs state)
//         // {
//         //     lock (this._queue)
//         //     {
//         //         //Log.Warning("NetworkClient PostToMainThread");
//         //         this._queue.Enqueue(() =>
//         //         {
//         //             //Log.Warning("NetworkClient PostToMainThread Callback");
//         //             callback?.Invoke(state);
//         //         });
//         //         //Log.Warning("NetworkClient PostToMainThread queue count:", this.queue.Count);
//         //     }
//         // }
//
//
//     }
//
// }