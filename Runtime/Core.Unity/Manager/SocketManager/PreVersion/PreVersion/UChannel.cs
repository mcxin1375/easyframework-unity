/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/


//using Easy.Framework;
//using System;
//using System.IO;
//using System.Net;
//using System.Net.Sockets;


//namespace Easy.Network
//{
//    public class UChannel : BaseChannel
//    {

//        private readonly CircularBuffer recvBuffer = new CircularBuffer();
//        //private readonly CircularBuffer sendBuffer = new CircularBuffer();

//        private byte[] cache;

//        private readonly PacketParser parser;
//        private readonly MemoryStream memoryStream;

//        private EndPoint ipEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);

//        private readonly byte[] packetSizeCache;

//        private Socket socket;

//        public UChannel(IPEndPoint ipEndPoint, Action<Action<SocketAsyncEventArgs>, SocketAsyncEventArgs> mainThreadAction) : base(mainThreadAction)
//        {
//            //this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//            //this.socket.NoDelay = true;


//            this.packetSizeCache = new byte[Packet.PACKET_HEAD];
//            this.memoryStream = new MemoryStream(ushort.MaxValue);
//            this.parser = new PacketParser(this.recvBuffer, this.memoryStream);
//            this.IPAddress = ipEndPoint;
//            this.cache = new byte[8192];

//        }
//        public override void Connect(IPEndPoint iPEndPoint)
//        {
//            base.Connect(iPEndPoint);

//            if (socket == null)
//            {
//                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//            }
//            OnNetworkState(ENetworkState.Connected);
//        }

//        public override void Disconnect()
//        {
//            base.Disconnect();

//            try
//            {
//                if (socket != null)
//                {
//                    if (socket.Connected)
//                    {
//                        socket.Shutdown(SocketShutdown.Both);
//                    }
//                    socket.Close();
//                    socket = null;
//                    //Log.Warning("------------- 主动断开 TChannel. Disconnected! ", RemoteAddress.ToString());
//                }
//            }
//            catch (Exception e)
//            {
//                Log.Warning("Error: ", e.ToString());
//            }
//        }
//        public override void Dispose()
//        {
//            base.Dispose();

//            Disconnect();
//            memoryStream.Dispose();
//        }

//        public override void Send(BasePacket message)
//        {
//            base.Send(message);

//            while (sendQueue.Count > 0)
//            {
//                BasePacket packet = sendQueue.Dequeue();

//                this.socket.SendTo(packet.packetBytes, 0, packet.packetBytes.Length, SocketFlags.None, this.IPAddress);
//            }
//        }


//        //public override void OnUpdate()
//        //{
//        //    this.UpdateRecv();
//        //    this.UpdateSend();
//        //}


//        public void UpdateRecv()
//        {
//            while (socket != null && this.socket.Available > 0)
//            {
//                int messageLength = 0;
//                try
//                {
//                    messageLength = this.socket.ReceiveFrom(this.cache, ref this.ipEndPoint);
//                }
//                catch (Exception e)
//                {
//                    Log.Warning(e);
//                    continue;
//                }

//                // 长度小于1，不是正常的消息
//                if (messageLength < 1)
//                {
//                    continue;
//                }

//                this.recvBuffer.Write(this.cache, 0, messageLength);
//                OnRecvComplete();
//            }
//        }

//        private void OnRecvComplete()
//        {
//            // 收到消息回调
//            while (true)
//            {
//                try
//                {
//                    if (!this.parser.Parse())
//                    {
//                        break;
//                    }
//                }
//                catch (Exception ee)
//                {
//                    Log.Error("UChannel Parse error: ", ee);
//                    OnSocketError(SocketError.SocketError);
//                    //NetworkState = ENetworkState.RecvError;
//                    return;
//                }

//                try
//                {
//                    this.OnReceive(this.parser.GetPacketHead(), parser.GetPacketBody());
//                }
//                catch (Exception ee)
//                {
//                    Log.Error(ee);
//                }
//            }

//        }


//        public void UpdateSend()
//        {
//            if (socket == null)
//            {
//                return;
//            }
//            if (sendQueue.Count == 0)
//            {
//                return;
//            }

//            BasePacket packet = sendQueue.Dequeue();

//            try
//            {
//                socket.SendTo(packet.packetBytes, 0, packet.packetBytes.Length, SocketFlags.None, IPAddress);
//            }
//            catch (Exception ex)
//            {
//                Log.Warning("UChannel SendTo: ", ex);
//            }
//        }


//    }
//}