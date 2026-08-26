// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2025/1/22
// // describe:
// //----------------------------------------------------------------*/
//
// using System.Collections.Generic;
//
// namespace EasyFramework
// {
//     public class SocketManager : Singleton<SocketManager>
//     {
// #if UNITY_EDITOR
//
//         internal List<ISocketChannel> DebugList = new();
//         
// #endif
//
//         public ISocketChannel CreateTChannel(ISocketHandler handler)
//         {
//             var channel = new SocketChannel(handler);
// #if UNITY_EDITOR
//             DebugList.Add(channel);
// #endif
//             return channel;
//         }
//     }
// }