// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace EasyFramework.Network
// {
//     public abstract class BaseProxy
//     {
//         public event Action<string> ON_ACTIVITY_GETLIST;
//         public string S2C_ACTIVITY_GETLIST_PACKET { get; private set; }
//         
//         private void S2C_ACTIVITY_GETLIST(string packet)
//         {
//             S2C_ACTIVITY_GETLIST_PACKET = packet;
//             S2C_ACTIVITY_GETLIST_EX(packet);
//             ON_ACTIVITY_GETLIST?.Invoke(packet);
//         }
//         protected virtual void S2C_ACTIVITY_GETLIST_EX(string packet) { }
//     }
// }