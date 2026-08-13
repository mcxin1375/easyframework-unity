// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
//
// using System;
// using System.Collections.Generic;
//
// namespace EasyFramework.Network
// {
//     public class WritePacket : BasePacket
//     {
//         // --- 包
//         // 2个字节 packetTag = 0x713b  标识符 
//         // 4个字节 packetLen           一个包的长度 
//         // 2个字节 packetCode          验证码 暂时填0
//         // 2个字节 msgId               包id
//         // --      pbBytes             pb包
//
//         public WritePacket(int msgId, byte[] pbBytes)
//         {
//             this.msgId = (short)msgId;
//             this.pbBytesLen = (short)pbBytes.Length;
//             this.pbBytes = pbBytes;
//
//             this.packetLen = 10;
//             this.packetLen += Convert.ToInt32(this.pbBytes.Length);
//
//             this.packetBytes = new byte[this.packetLen];
//
//             Array.Copy(GetBigEndBytes(packetTag), 0, this.packetBytes, 0, 2);
//             Array.Copy(GetBigEndBytes(this.packetLen), 0, this.packetBytes, 2, 4);
//             Array.Copy(GetBigEndBytes(this.packetCode), 0, this.packetBytes, 6, 2);
//             Array.Copy(GetBigEndBytes(this.msgId), 0, this.packetBytes, 8, 2);
//             Array.Copy(this.pbBytes, 0, this.packetBytes, 10, this.pbBytes.Length);
//
//             //List<byte> temp = new List<byte>();
//             //Log.Info(PacketLen, MsgId, ByteLen);
//             //temp.AddRange(GetBigEndBytes(PacketLen));
//             //temp.AddRange(GetBigEndBytes(MsgId));
//             //temp.AddRange(GetBigEndBytes(ByteLen));
//             //temp.AddRange(Bytes);
//             //PacketBytes = temp.ToArray();
//
//             //Log.Info("MsgId:", msgid, "PacketLen:", PacketLen, "PacketBytes:", PacketBytes.Length);
//         }
//
//         public static byte[] ToBytes(int msgId, byte[] pbBytes)
//         {
//             int packetLen = Convert.ToInt32(pbBytes.Length) + 10;
//             byte[] bytes = new byte[packetLen];
//             
//             Array.Copy(GetBigEndBytes(packetTag), 0, bytes, 0, 2);
//             Array.Copy(GetBigEndBytes(packetLen), 0, bytes, 2, 4);
//             Array.Copy(GetBigEndBytes((short)0), 0, bytes, 6, 2);
//             Array.Copy(GetBigEndBytes(msgId), 0, bytes, 8, 2);
//             Array.Copy(pbBytes, 0, bytes, 10, pbBytes.Length);
//
//             return bytes;
//             
//             // List<byte> temp = new List<byte>();
//             // temp.AddRange(GetBigEndBytes(packetTag));
//             // temp.AddRange(GetBigEndBytes(packetLen));
//             // temp.AddRange(GetBigEndBytes((short)0));
//             // temp.AddRange(GetBigEndBytes((short)msgId));
//             // temp.AddRange(pbBytes);
//             // // Log.Info(packetLen, temp.Count);
//             // return temp.ToArray();
//         }
//
//         /*
//         public void WriteByte(byte bValue)
//         {
//             ushort nSize = sizeof(byte);
//             if ((PacketLen + nSize) > LenLimit)
//             {
//                 Log.Info("WriteByte error!too Large!");
//                 return;
//             }
//             byte[] bytes = new byte[1];
//             bytes[0] = bValue;
//             AddData(bytes);
//         }
//
//         public void WriteBytes(byte[] bValue)
//         {
//             if ((PacketLen + bValue.Length) > LenLimit)
//             {
//                 Log.Info("WriteByte error!too Large!");
//                 return;
//             }
//             AddData(bValue);
//         }
//
//         public void WriteShort(short nValue)
//         {
//             ushort nSize = sizeof(short);
//             if (PacketLen + nSize > LenLimit)
//             {
//                 Log.Info("WriteShort error!too Large!");
//                 return;
//             }
//             byte[] bytes = HostToNetwork ? BitConverter.GetBytes(IPAddress.HostToNetworkOrder(nValue)) : BitConverter.GetBytes(nValue);
//             AddData(bytes);
//         }
//
//         public void WriteUShort(ushort nValue)
//         {
//             ushort nSize = sizeof(ushort);
//             if (PacketLen + nSize > LenLimit)
//             {
//                 Log.Info("WriteShort error!too Large!");
//                 return;
//             }
//             byte[] bytes = BitConverter.GetBytes(nValue);
//             if (BitConverter.IsLittleEndian)
//             {
//                 Array.Reverse(bytes);
//             }
//             AddData(bytes);
//         }
//
//         //Wite int
//         public void WriteInt(int nValue)
//         {
//             ushort nSize = sizeof(int);
//             if (PacketLen + nSize > LenLimit)
//             {
//                 Log.Info("WriteInt error!too Large!");
//                 return;
//             }
//             byte[] bytes = HostToNetwork ? BitConverter.GetBytes(IPAddress.HostToNetworkOrder(nValue)) : BitConverter.GetBytes(nValue);
//             AddData(bytes);
//         }
//
//         public void WriteInt64(long lValue)
//         {
//             ushort nSize = sizeof(long);
//             if (PacketLen + nSize > LenLimit)
//             {
//                 Log.Info("WriteInt64 error!too Large!");
//                 return;
//             }
//             byte[] bytes = HostToNetwork ? BitConverter.GetBytes(IPAddress.HostToNetworkOrder(lValue)) : BitConverter.GetBytes(lValue);
//             AddData(bytes);
//         }
//
//         public void WriteBoolean(Boolean bValue)
//         {
//             ushort nSize = sizeof(Boolean);
//             if (PacketLen + nSize > LenLimit)
//             {
//                 Log.Info("WriteBoolean error!too Large!");
//                 return;
//             }
//             byte[] bytes = BitConverter.GetBytes(bValue);
//             AddData(bytes);
//         }
//
//         public void WriteString(String strValue)
//         {
//             if (strValue == null)
//             {
//                 strValue = "";
//             }
//
//             byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strValue);
//             if (PacketLen + bytes.Length > LenLimit)
//             {
//                 Log.Info("WriteString error!too Large!");
//                 return;
//             }
//             //写入字符串的长度
//             WriteShort((short)bytes.Length);
//             AddData(bytes);
//         }
//         */
//
//     }
// }