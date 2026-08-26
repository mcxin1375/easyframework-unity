// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
//
// using System;
// using System.IO;
//
// namespace EasyFramework.Network
// {
//     public enum ParserState
//     {
//         PacketSize,
//         PacketBody
//     }
//
//     public static class Packet
//     {
//         public const int PACKET_HEAD = 10;      //包头总长度
//     }
//
//     public class PacketParser
//     {
//         private readonly ReceiveBuffer buffer;
//         private int pbBytesLen;
//         private ParserState state;
//         public MemoryStream memoryStream;
//         private byte[] headBody;
//         private bool isOK;
//         //private readonly int packetSizeLength;
//
//         public PacketParser(ReceiveBuffer buffer, MemoryStream memoryStream)
//         {
//             this.buffer = buffer;
//             this.memoryStream = memoryStream;
//             headBody = new byte[Packet.PACKET_HEAD];
//         }
//
//         public bool Parse(out byte[] headBytes, out byte[] dataBytes)
//         {
//             headBytes = null;
//             dataBytes = null;
//
//             if (state == ParserState.PacketSize)
//             {
//                 if (buffer.Length < Packet.PACKET_HEAD)
//                     return false;
//
//                 buffer.Read(headBody, 0, Packet.PACKET_HEAD);
//
//                 //int tag = (headBody[0] << 8) + headBody[1];
//                 int len = Endian.SwapInt32(BitConverter.ToInt32(headBody, 2));
//                 //int code = (headBody[6] << 8) + headBody[7];
//                 //int msgId = (headBody[8] << 8) + headBody[9];
//
//                 pbBytesLen = len - 10;
//                 state = ParserState.PacketBody;
//             }
//             if (state == ParserState.PacketBody)
//             {
//                 if (buffer.Length < pbBytesLen)
//                     return false;
//
//                 //memoryStream.Seek(0, SeekOrigin.Begin);
//                 //memoryStream.SetLength(pbBytesLen);
//                 //byte[] bytes = memoryStream.GetBuffer();
//                 //buffer.Read(bytes, 0, pbBytesLen);
//
//                 byte[] bytes = new byte[pbBytesLen];
//                 buffer.Read(bytes, 0, pbBytesLen);
//
//                 state = ParserState.PacketSize;
//
//                 headBytes = headBody;
//                 dataBytes = bytes;
//                 return true;
//             }
//             return false;
//         }
//
//         public bool Parse()
//         {
//             if (this.isOK)
//             {
//                 return true;
//             }
//
//             bool finish = false;
//             while (!finish)
//             {
//                 switch (this.state)
//                 {
//                     case ParserState.PacketSize:
//                         //Log.Info("PacketParser", this.buffer.Length, Packet.PACKET_HEAD);
//                         if (this.buffer.Length < Packet.PACKET_HEAD)
//                         {
//                             finish = true;
//                         }
//                         else
//                         {
//                             this.buffer.Read(headBody, 0, Packet.PACKET_HEAD);
//
//                             //int tag = (headBody[0] << 8) + headBody[1];
//                             int len = Endian.SwapInt32(BitConverter.ToInt32(headBody, 2));
//                             //int code = (headBody[6] << 8) + headBody[7];
//                             //int msgId = (headBody[8] << 8) + headBody[9];
//
//                             pbBytesLen = len - 10;
//                             this.state = ParserState.PacketBody;
//                         }
//                         break;
//                     case ParserState.PacketBody:
//                         //Log.Info("packetSize:", packetSize, "buffer.Length:", buffer.Length);
//                         if (this.buffer.Length < this.pbBytesLen)
//                         {
//                             finish = true;
//                         }
//                         else
//                         {
//                             this.memoryStream.Seek(0, SeekOrigin.Begin);
//                             this.memoryStream.SetLength(this.pbBytesLen);
//                             byte[] bytes = this.memoryStream.GetBuffer();
//                             this.buffer.Read(bytes, 0, this.pbBytesLen);
//                             this.isOK = true;
//                             this.state = ParserState.PacketSize;
//                             finish = true;
//                         }
//                         break;
//                 }
//             }
//             return this.isOK;
//         }
//
//         public byte[] GetPacketHead()
//         {
//             this.isOK = false;
//             return headBody;
//         }
//         public byte[] GetPacketBody()
//         {
//             byte[] dataBody = new byte[pbBytesLen];
//             this.memoryStream.Read(dataBody, 0, pbBytesLen);
//             return dataBody;
//         }
//     }
// }