// /*----------------------------------------------------------------
// // author: Cookie(mcx)
// // date: 2023/7/18
// // describe:
// //----------------------------------------------------------------*/
//
//
// using System;
// using System.Net;
// using System.Net.Sockets;
// using System.Collections.Generic;
//
// namespace EasyFramework.Network
// {
//     public enum NetworkChannelType
//     {
//         TCP,
//         UDP
//     }
//     public abstract class BaseChannel
//     {
//         public event Action<byte[], byte[]> OnReceiveAction;
//         public event Action<ENetworkState> OnNetworkStateAction;
//         public event Action<SocketError> OnSocketErrorAction;
//         
//         public bool IsConnect => NetworkState == ENetworkState.Connected;
//         public IPEndPoint IPAddress { get; private set; }
//         public NetworkChannelType NetworkChannelType { get; private set; }
//
//         private ENetworkState _networkState = ENetworkState.Disconnected;
//         public ENetworkState NetworkState {
//             get => _networkState;
//             protected set
//             {
//                 _networkState = value;
//                 OnNetworkStateAction?.Invoke(_networkState);
//             }
//         }
//         
//         private SocketError _socketError = SocketError.SocketError;
//         public SocketError SocketError {
//             get => _socketError;
//             protected set
//             {
//                 _socketError = value;
//                 OnSocketErrorAction?.Invoke(_socketError);
//             }
//         }
//
//         protected readonly Queue<BasePacket> SendQueue = new Queue<BasePacket>();
//         
//
//         public BaseChannel(NetworkChannelType networkChannelType)
//         {
//             NetworkChannelType = networkChannelType;
//             NetworkState = ENetworkState.Disconnected;
//         }
//         
//         public void Connect(IPEndPoint ipEndPoint)
//         {
//             IPAddress = ipEndPoint;
//             NetworkState = ENetworkState.Connecting;
//             RequestConnect(ipEndPoint);
//         }
//
//         public void Disconnect()
//         {
//             SendQueue?.Clear();
//             RequestDisconnect();
//         }
//
//         public void Send(BasePacket message)
//         {
//             SendQueue.Enqueue(message);
//             OnSend(message);
//         }
//         
//         protected abstract void RequestConnect(IPEndPoint ipEndPoint);
//         protected abstract void RequestDisconnect();
//         protected abstract void OnSend(BasePacket message);
//
//         protected void OnReceive(byte[] headBody, byte[] dataBody)
//         {
//             OnReceiveAction?.Invoke(headBody, dataBody);
//         }
//
//     }
// }