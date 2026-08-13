// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
//
// namespace EasyFramework.Network
// {
//     public static class Endian
//     {
//         public static short SwapInt16(this short n)
//         {
//             return (short)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
//         }
//
//         public static ushort SwapUInt16(this ushort n)
//         {
//             return (ushort)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
//         }
//
//         public static int SwapInt32(this int n)
//         {
//             return (int)(((SwapInt16((short)n) & 0xffff) << 0x10) |
//                           (SwapInt16((short)(n >> 0x10)) & 0xffff));
//         }
//
//         public static uint SwapUInt32(this uint n)
//         {
//             return (uint)(((SwapUInt16((ushort)n) & 0xffff) << 0x10) |
//                            (SwapUInt16((ushort)(n >> 0x10)) & 0xffff));
//         }
//
//         public static long SwapInt64(this long n)
//         {
//             return (long)(((SwapInt32((int)n) & 0xffffffffL) << 0x20) |
//                            (SwapInt32((int)(n >> 0x20)) & 0xffffffffL));
//         }
//
//         public static ulong SwapUInt64(this ulong n)
//         {
//             return (ulong)(((SwapUInt32((uint)n) & 0xffffffffL) << 0x20) |
//                             (SwapUInt32((uint)(n >> 0x20)) & 0xffffffffL));
//         }
//     }
//
//     public class ReadPacket : BasePacket
//     {
//         // --- ��
//         // 2���ֽ� packetTag = 0x713b  ��ʶ�� 
//         // 4���ֽ� packetLen           һ�����ĳ��� 
//         // 2���ֽ� packetCode          ��֤�� ��ʱ��0
//         // 2���ֽ� msgId               ��id
//         // --      pbBytes             pb��
//
//         public ReadPacket(byte[] headBytes, byte[] bytes)
//         {
//             //Log.Info("ReadPacket", headBytes.Length, bytes.Length);
//             // this.packetLen = Endian.SwapInt32(BitConverter.ToInt32(headBytes, 2));
//             this.packetLen = BitConverter.ToInt32(headBytes, 2);
//             this.packetCode = (short)((headBytes[6] << 8) + headBytes[7]);
//             this.msgId = (short)((headBytes[8] << 8) + headBytes[9]);
//
//             pbBytes = bytes;
//         }
//
//         /*
//         public byte ReadByte()
//         {
//             int nSize = sizeof(byte);
//             if (curPos + nSize > Packet.PACKET_HEAD)
//             {
//                 Log.Info("ReadByte error!");
//                 return 0;
//             }
//             var readData = HeadBody[curPos];
//             curPos += nSize;
//             return readData;
//         }
//         //public byte[] ReadBytes(int len)
//         //{
//         //    if (curPos + len > PacketLen)
//         //    {
//         //        Log.Info("ReadBytes error!");
//         //        return null;
//         //    }
//
//         //    var readData = headBody.ToList().GetRange(curPos, len);
//         //    curPos += len;
//         //    return readData.ToArray();
//         //}
//
//         public short ReadShort()
//         {
//             int nSize = sizeof(short);
//
//             if (curPos + nSize > Packet.PACKET_HEAD)
//             {
//                 Log.Info("ReadShort error!");
//                 return 0;
//             }
//             var readData = BitConverter.ToInt16(HeadBody, curPos);
//             curPos += nSize;
//             return HostToNetwork ? IPAddress.NetworkToHostOrder(readData) : readData;
//         }
//
//         public ushort ReadUShort()
//         {
//             int nSize = sizeof(ushort);
//
//             if (curPos + nSize > Packet.PACKET_HEAD)
//             {
//                 Log.Info("ReadShort error!");
//                 return 0;
//             }
//             var readData = BitConverter.ToUInt16(HeadBody, curPos);
//             curPos += nSize;
//             return readData;
//         }
//
//         public int ReadInt()
//         {
//             int nSize = sizeof(Int32);
//             if (curPos + nSize > Packet.PACKET_HEAD)
//             {
//                 Log.Info("ReadInt error!");
//                 return 0;
//             }
//             var readData = BitConverter.ToInt32(HeadBody, curPos);
//             curPos += nSize;
//             return HostToNetwork ? IPAddress.NetworkToHostOrder(readData) : readData;
//         }
//
//         public long ReadInt64()
//         {
//             int nSize = sizeof(Int64);
//             if (curPos + nSize > Packet.PACKET_HEAD)
//             {
//                 Log.Info("ReadInt64 error!");
//                 return 0;
//             }
//             long readData = BitConverter.ToInt64(HeadBody, curPos);
//             curPos += nSize;
//             return HostToNetwork ? IPAddress.NetworkToHostOrder(readData) : readData;
//         }
//
//         public Boolean ReadBoolean()
//         {
//             int nSize = sizeof(Boolean);
//             if (curPos + nSize > Packet.PACKET_HEAD)
//             {
//                 Log.Info("ReadBoolean error!");
//                 return false;
//             }
//             var readData = BitConverter.ToBoolean(HeadBody, curPos);
//             curPos += nSize;
//             return readData;
//         }
//
//         public String ReadString()
//         {
//             int nSize = ReadShort();
//             if (curPos + nSize > Packet.PACKET_HEAD)
//             {
//                 Log.Info("ReadString error!");
//                 return "";
//             }
//             byte[] strs = new byte[nSize];
//             for (int i = 0; i < nSize; i++)
//             {
//                 strs[i] = HeadBody[curPos + i];
//             }
//             var readData = Encoding.UTF8.GetString(strs);
//             curPos += nSize;
//             return readData;
//         }
//         */
//
//     }
// }