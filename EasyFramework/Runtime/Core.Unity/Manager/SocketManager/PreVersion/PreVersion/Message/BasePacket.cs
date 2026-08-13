// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
//
// using System;
//
// namespace EasyFramework.Network
// {
//     public class BasePacket
//     {
//
//         // --- 包
//         // 2个字节 packetTag = 0x713b  标识符 
//         // 4个字节 packetLen           一个包的长度 
//         // 2个字节 packetCode          验证码 暂时填0
//         // 2个字节 msgId               包id
//         // --      pbBytes             pb包
//
//         /// <summary>
//         /// 标识符，固定常量
//         /// </summary>
//         public const short packetTag = 0x713b;
//         /// <summary>
//         /// 包长度
//         /// </summary>
//         public int packetLen { protected set; get; }
//         /// <summary>
//         /// 验证码
//         /// </summary>
//         public short packetCode { protected set; get; }
//         /// <summary>
//         /// 协议ID
//         /// </summary>
//         public short msgId { protected set; get; }
//         /// <summary>
//         /// PB数据长度
//         /// </summary>
//         public short pbBytesLen { protected set; get; }
//         /// <summary>
//         /// PB数据
//         /// </summary>
//         public byte[] pbBytes { protected set; get; }
//
//         /// <summary>
//         /// 所有的数据
//         /// </summary>
//         public byte[] packetBytes { protected set; get; }
//
//         /// <summary>
//         /// 转成大端的字节数组
//         /// </summary>
//         /// <param name="num"></param>
//         /// <returns></returns>
//         protected static byte[] GetBigEndBytes(short num)
//         {
//             byte[] bs = BitConverter.GetBytes(num);
//             if (BitConverter.IsLittleEndian)
//             {
//                 Array.Reverse(bs);
//             }
//             return bs;
//         }
//
//         protected static byte[] GetBigEndBytes(int num)
//         {
//             byte[] bs = BitConverter.GetBytes(num);
//             if (BitConverter.IsLittleEndian)
//             {
//                 Array.Reverse(bs);
//             }
//             return bs;
//         }
//
//         //protected bool HostToNetwork = false; //主机序转换网络序
//
//         //private List<byte> dataBuffers = new List<byte>();
//
//         //protected void AddData(byte[] bytes)
//         //{
//         //    dataBuffers.AddRange(bytes);
//         //    ChangePocketLen();
//         //}
//
//         /// <summary>
//         /// 修改包长度
//         /// </summary>
//         //protected void RefreshPocketLen()
//         //{
//         //    if (dataBuffers.Count < 6) return;
//
//         //    PacketLen = dataBuffers.Count;
//         //    byte[] bytes = HostToNetwork ? BitConverter.GetBytes(IPAddress.HostToNetworkOrder(PacketLen)) : BitConverter.GetBytes(PacketLen);
//
//         //    for (int i = 2; i < 6; i++)
//         //    {
//         //        dataBuffers[i] = bytes[i - 2];
//         //    }
//
//         //Log.Info("bytes.Length: ", bytes.Length);
//         //List<byte> tmpList = new List<byte>();
//         //for (int i = 0; i < dataBuffers.Count; i++)
//         //{
//         //    if (i > 1 && i < 6 && i - 2 < bytes.Length) //i=2, 3
//         //    {
//         //        tmpList.Add(bytes[i - 2]);
//         //    }
//         //    else
//         //    {
//         //        tmpList.Add(dataBuffers[i]);
//         //    }
//         //}
//         //dataBuffers.Clear();
//         //dataBuffers = tmpList;
//
//         //            byte[] arr = tmpList.GetRange(2, 2).ToArray();
//         //            int packetLen = BitConverter.ToInt16(arr, 0);
//         //            Log.Debug("packetLen: " + packetLen);
//         //}
//
//     }
// }
