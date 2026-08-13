/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/


//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Net.Sockets;
//using Easy.Framework;


//namespace Easy.Network
//{
//    public class UChannelAsync : BaseChannel
//    {

//        private SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
//        private SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();

//        private readonly CircularBuffer recvBuffer = new CircularBuffer();
//        //private readonly CircularBuffer sendBuffer = new CircularBuffer();
//        private BasePacket sendCache;

//        private byte[] cache;

//        private readonly PacketParser parser;
//        private readonly MemoryStream memoryStream;


//        private readonly byte[] packetSizeCache;
//        private bool isSending;
//        private bool isRecving;

//        private Socket socket;


//        public UChannelAsync(IPEndPoint ipEndPoint, Action<Action<SocketAsyncEventArgs>, SocketAsyncEventArgs> mainThreadAction) : base(mainThreadAction)
//        {
//            //this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//            //this.socket.NoDelay = true;

//            this.innArgs.Completed += this.OnComplete;
//            this.outArgs.Completed += this.OnComplete;

//            this.packetSizeCache = new byte[Packet.PACKET_HEAD];
//            this.memoryStream = new MemoryStream(ushort.MaxValue);
//            this.parser = new PacketParser(this.recvBuffer, this.memoryStream);
//            this.IPAddress = ipEndPoint;
//            this.cache = new byte[8192];

//            this.isSending = false;
//            this.isRecving = false;
//        }

//        //public override void Start()
//        //{
//        //    if (!this.isRecving)
//        //    {
//        //        this.isRecving = true;
//        //        this.StartRecv();
//        //    }
//        //}

//        public override void OnUpdate()
//        {

//        }

//        public override void Send(BasePacket message)
//        {
//        }

//        //public override void Connect()
//        //{
//        //    this.ConnectAsync(this.IPAddress);
//        //}

//        //public override void Disconnect()
//        //{
//        //    if (this.socket != null)
//        //    {
//        //        //if (NetworkState == ENetworkState.Connected)
//        //        //{
//        //            //this.socket.Shutdown(SocketShutdown.Both);
//        //        //}
//        //        this.socket.Close();
//        //        this.socket = null;
//        //        this.isSending = false;
//        //        this.isRecving = false;
//        //    }
//        //}

//        public void ConnectAsync(IPEndPoint ipEndPoint)
//        {
//            this.innArgs.RemoteEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);
//            this.outArgs.RemoteEndPoint = ipEndPoint;
//            //if (this.socket.ConnectAsync(this.outArgs))
//            //{
//            //    return;
//            //}
//            OnConnectComplete(this.outArgs);
//        }


//        private void OnComplete(object sender, SocketAsyncEventArgs e)
//        {
//            //Log.Info("TChannel OnComplete: ", e.LastOperation);
//            switch (e.LastOperation)
//            {
//                case SocketAsyncOperation.Connect:
//                    //MainThreadManager.Instance.Post(this.OnConnectComplete, e);
//                    break;
//                //case SocketAsyncOperation.Receive:
//                case SocketAsyncOperation.ReceiveFrom:
//                    //MainThreadManager.Instance.Post(this.OnRecvComplete, e);
//                    break;
//                //case SocketAsyncOperation.Send:
//                case SocketAsyncOperation.SendTo:
//                    //MainThreadManager.Instance.Post(this.OnSendComplete, e);
//                    break;
//                case SocketAsyncOperation.Disconnect:
//                    //MainThreadManager.Instance.Post(this.OnDisconnectComplete, e);
//                    break;
//                default:
//                    throw new Exception("socket error: " + e.LastOperation);
//            }
//        }

//        private void OnConnectComplete(object o)
//        {
//            if (this.socket == null)
//            {
//                return;
//            }

//            //e.RemoteEndPoint = null;

//            //Log.Info("------------- TChannel connect succeed! ", RemoteAddress.ToString());
//            //this.Start();
//        }

//        private void OnDisconnectComplete(SocketAsyncEventArgs o)
//        {
//            //SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
//            //this.OnError((int)e.SocketError);
//        }

//        // 接收数据
//        private void StartRecv()
//        {
//            //int size = this.recvBuffer.ChunkSize - this.recvBuffer.LastIndex;
//            //this.RecvAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex, size);
//            this.RecvAsync(this.cache, 0, this.cache.Length);
//        }

//        public void RecvAsync(byte[] buffer, int offset, int count)
//        {
//            try
//            {
//                this.innArgs.SetBuffer(buffer, offset, count);
//            }
//            catch (Exception e)
//            {
//                Log.Warning("RecvAsync: ", e);
//                //throw new Exception($"socket set buffer error: {buffer.Length}, {offset}, {count}", e);
//            }

//            if (this.socket.ReceiveFromAsync(this.innArgs))
//            {
//                return;
//            }
//            OnRecvComplete(this.innArgs);
//        }

//        private void OnRecvComplete(object o)
//        {
//            //Log.Info("OnRecvComplete .1");
//            if (this.socket == null)
//            {
//                return;
//            }
//            SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;

//            if (e.SocketError != SocketError.Success)
//            {
//                //this.OnError((int)e.SocketError);
//                //NetworkState = ENetworkState.RecvError;
//                this.StartRecv();
//                return;
//            }

//            if (e.BytesTransferred == 0)
//            {
//                //this.OnError(ErrorCode.ERR_PeerDisconnect);
//                //Log.Info("BytesTransferred");
//                this.StartRecv();
//                return;
//            }
//            //Log.Info("OnRecvComplete .2");

//            this.recvBuffer.Write(this.cache, 0, e.BytesTransferred);

//            //this.recvBuffer.LastIndex += e.BytesTransferred;
//            //if (this.recvBuffer.LastIndex == this.recvBuffer.ChunkSize)
//            //{
//            //    this.recvBuffer.AddLast();
//            //    this.recvBuffer.LastIndex = 0;
//            //}

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
//                    return;
//                }

//                try
//                {
//                    this.OnReceive(this.parser.GetPacketHead(), this.parser.GetPacketBody());
//                }
//                catch (Exception ee)
//                {
//                    Log.Error(ee);
//                }
//            }

//            if (this.socket == null)
//            {
//                return;
//            }

//            this.StartRecv();
//        }


//        public void StartSend()
//        {
//            if (isSending)
//                return;

//            if (sendCache == null)
//            {
//                // 没有数据需要发送
//                if (this.sendQueue.Count == 0)
//                {
//                    this.isSending = false;
//                    return;
//                }
//                sendCache = sendQueue.Dequeue();
//            }
//            this.isSending = true;
//            this.SendAsync(sendCache.packetBytes, 0, sendCache.packetBytes.Length);
//        }

//        public void SendAsync(byte[] buffer, int offset, int count)
//        {
//            try
//            {
//                this.outArgs.SetBuffer(buffer, offset, count);
//            }
//            catch (Exception ex)
//            {
//                Log.Warning(ex);
//                //throw new Exception("socket set buffer error: ", buffer.Length, offset, count, e);
//            }
//            if (this.socket.SendToAsync(this.outArgs))
//            {
//                return;
//            }
//            OnSendComplete(this.outArgs);
//        }

//        private void OnSendComplete(object o)
//        {
//            if (this.socket == null)
//            {
//                return;
//            }
//            SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;

//            if (e.SocketError != SocketError.Success)
//            {
//                //this.OnError((int)e.SocketError);
//                //NetworkState = ENetworkState.SendError;
//                isSending = false;
//                sendCache = null;
//                this.StartSend();
//                return;
//            }

//            //if (e.BytesTransferred == 0)
//            //{
//            //this.OnError(ErrorCode.ERR_PeerDisconnect);
//            //return;
//            //}

//            isSending = false;
//            sendCache = null;
//            this.StartSend();
//        }


//        public override void Dispose()
//        {
//            base.Dispose();

//            if (this.socket != null)
//            {
//                this.socket.Close();
//                this.socket = null;
//            }
//            this.sendQueue.Clear();
//            this.innArgs.Dispose();
//            this.outArgs.Dispose();
//            this.innArgs = null;
//            this.outArgs = null;
//            this.memoryStream.Dispose();
//        }


//    }
//}