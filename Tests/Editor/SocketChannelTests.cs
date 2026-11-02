using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EasyFramework.Tests
{
    public class SocketChannelTests
    {
        private static readonly FieldInfo SocketField = typeof(SocketChannel).GetField(
            "_socket", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ReceiveBufferField = typeof(SocketChannel).GetField(
            "_receiveBuffer", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo BufferStorageField = typeof(RingBuffer).GetField(
            "_buffer", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo TickMethod = typeof(ETask).GetMethod(
            "Tick", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly List<SocketChannel> _channels = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var channel in _channels)
            {
                try { channel.Dispose(); }
                finally { SocketOf(channel).Dispose(); }
            }
            _channels.Clear();
            Tick();
            Tick();
        }

        [Test]
        public void Dispose_UnconnectedSocket_ClosesNativeHandle()
        {
            var channel = NewChannel();
            var handle = HandleOf(channel);
            Assert.That(handle.IsClosed, Is.False);

            channel.Dispose();

            Assert.That(handle.IsClosed, Is.True);
            Assert.That(channel.IsConnected, Is.False);
        }

        [Test]
        public void Dispose_RepeatedCalls_AreSafe()
        {
            var channel = NewChannel();
            var handle = HandleOf(channel);
            channel.Dispose();

            Assert.DoesNotThrow(() => channel.Dispose());
            Assert.DoesNotThrow(() => channel.Disconnect());
            Assert.DoesNotThrow(() => channel.Send(new byte[] { 1 }));
            Assert.DoesNotThrow(() => channel.Send());
            Assert.That(handle.IsClosed, Is.True);
        }

        [UnityTest]
        public IEnumerator Dispose_AfterConnectionFailure_ClosesNativeHandle()
        {
            // Reserving without listening makes the refusal local and prevents a port-reuse race.
            using var reserved = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            reserved.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            var handler = new ProbeHandler();
            var channel = NewChannel(handler);
            var handle = HandleOf(channel);
            bool connected = true;

            yield return Await(channel.ConnectAsync("127.0.0.1", ((IPEndPoint)reserved.LocalEndPoint).Port), result => connected = result);
            Assert.That(connected, Is.False);
            yield return Until(() => Volatile.Read(ref handler.ErrorCount) == 1, "The connection error was not reported.");
            Assert.That(Volatile.Read(ref handler.ErrorCount), Is.EqualTo(1));

            channel.Dispose();

            Assert.That(handle.IsClosed, Is.True);
            Assert.That(channel.IsConnected, Is.False);
        }

        [UnityTest]
        public IEnumerator Dispose_ConnectedWithPendingReceive_ClosesNativeHandle()
        {
            yield return DisposeConnected(false);
        }

        [UnityTest]
        public IEnumerator Dispose_ConnectedWithPendingSendAndReceive_ClosesNativeHandle()
        {
            yield return DisposeConnected(true);
        }

        private IEnumerator DisposeConnected(bool send)
        {
            var listener = Listen();
            Socket peer = null;
            try
            {
                var handler = new ProbeHandler();
                var channel = NewChannel(handler);
                yield return Connect(channel, listener, socket => peer = socket);
                var handle = HandleOf(channel);
                if (send)
                {
                    SocketOf(channel).SendBufferSize = 1024;
                    peer.ReceiveBufferSize = 1024;
                    channel.Send(new byte[1024 * 1024]);
                }

                channel.Dispose();

                Assert.That(handle.IsClosed, Is.True);
                Assert.That(channel.IsConnected, Is.False);
                Assert.DoesNotThrow(() => channel.Dispose());
                Assert.That(Volatile.Read(ref handler.ErrorCount), Is.Zero);
                yield return Until(() => BuffersReturned(channel), "Disposal did not return both pooled buffers after pending I/O ended.");
            }
            finally
            {
                peer?.Dispose();
                listener.Stop();
            }
        }

        [UnityTest]
        public IEnumerator Dispose_AfterDisconnectAsync_ClosesNativeHandle()
        {
            var listener = Listen();
            Socket peer = null;
            try
            {
                var channel = NewChannel();
                yield return Connect(channel, listener, socket => peer = socket);
                var handle = HandleOf(channel);
                bool connected = true;

                yield return Await(channel.DisconnectAsync(), result => connected = result);
                Assert.That(connected, Is.False);
                Assert.That(channel.IsConnected, Is.False);
                channel.Dispose();

                Assert.That(handle.IsClosed, Is.True);
                Assert.DoesNotThrow(() => channel.Dispose());
            }
            finally
            {
                peer?.Dispose();
                listener.Stop();
            }
        }

        [UnityTest]
        public IEnumerator Dispose_DuringConnect_CompletesConnectionWait()
        {
            var listener = Listen();
            try
            {
                for (int i = 0; i < 16; i++)
                {
                    var channel = NewChannel();
                    var handle = HandleOf(channel);
                    var connecting = channel.ConnectAsync("127.0.0.1", PortOf(listener));
                    channel.Dispose();

                    // Completion may win before disposal; every pending wait must still settle.
                    yield return Await(connecting);
                    Assert.That(handle.IsClosed, Is.True);
                    Assert.That(channel.IsConnected, Is.False);
                }
            }
            finally { listener.Stop(); }
        }

        [UnityTest]
        public IEnumerator ConnectAsync_AfterDispose_ReturnsFalse()
        {
            var channel = NewChannel();
            channel.Dispose();
            bool connected = true;

            yield return Await(channel.ConnectAsync("127.0.0.1", 9), result => connected = result);

            Assert.That(connected, Is.False);
            Assert.That(channel.IsConnected, Is.False);
        }

        [Test]
        public void Dispose_ConcurrentCalls_AreSafe()
        {
            var channel = NewChannel();
            var handle = HandleOf(channel);

            Assert.DoesNotThrow(() => Parallel.For(0, 16, _ => channel.Dispose()));

            Assert.That(handle.IsClosed, Is.True);
            Assert.That(channel.IsConnected, Is.False);
        }

        [UnityTest]
        public IEnumerator Dispose_FromReceiveCallback_StopsFurtherReceiving()
        {
            var listener = Listen();
            Socket peer = null;
            try
            {
                using var completed = new ManualResetEventSlim();
                var handler = new ProbeHandler();
                var channel = NewChannel(handler);
                var handle = HandleOf(channel);
                Exception callbackError = null;
                handler.OnReceived = () =>
                {
                    try { channel.Dispose(); }
                    catch (Exception exception) { callbackError = exception; }
                    finally { completed.Set(); }
                };
                yield return Connect(channel, listener, socket => peer = socket);

                Assert.That(peer.Send(new byte[] { 42 }), Is.EqualTo(1));
                yield return Until(() => completed.IsSet, "The receive callback did not complete.");

                Assert.That(callbackError, Is.Null);
                Assert.That(handle.IsClosed, Is.True);
                Assert.That(channel.IsConnected, Is.False);
                Assert.That(Volatile.Read(ref handler.ReceiveCount), Is.EqualTo(1));
                Assert.DoesNotThrow(() => channel.Dispose());
            }
            finally
            {
                peer?.Dispose();
                listener.Stop();
            }
        }

        [UnityTest]
        public IEnumerator Dispose_WhileReceiveHandlerActive_PreservesItsBorrowedBuffer()
        {
            var listener = Listen();
            Socket peer = null;
            SocketChannel channel = null;
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var completed = new ManualResetEventSlim();
            Exception callbackError = null;
            byte received = 0;
            try
            {
                var handler = new ProbeHandler();
                channel = NewChannel(handler);
                var handle = HandleOf(channel);
                handler.ReceiveOverride = buffer =>
                {
                    try
                    {
                        entered.Set();
                        if (!release.Wait(5000)) throw new TimeoutException("The receive handler was not released.");
                        if (!buffer.TryPeek(1, out var bytes)) throw new InvalidOperationException("Disposal cleared the active receive buffer.");
                        received = bytes[0];
                        buffer.AdvanceRead(1);
                    }
                    catch (Exception exception) { callbackError = exception; }
                    finally { completed.Set(); }
                };
                yield return Connect(channel, listener, socket => peer = socket);
                Assert.That(peer.Send(new byte[] { 42 }), Is.EqualTo(1));
                yield return Until(() => entered.IsSet, "The receive handler did not start.");

                channel.Dispose();

                Assert.That(handle.IsClosed, Is.True);
                Assert.That(channel.IsConnected, Is.False);
                release.Set();
                yield return Until(() => completed.IsSet, "The active receive handler did not exit after disposal.");
                Assert.That(callbackError, Is.Null);
                Assert.That(received, Is.EqualTo(42));
                Assert.DoesNotThrow(() => channel.Dispose());
                yield return Until(() => BuffersReturned(channel), "Disposal did not return both pooled buffers after the receive handler exited.");
            }
            finally
            {
                channel?.Dispose();
                release.Set();
                if (entered.IsSet) completed.Wait(5000);
                peer?.Dispose();
                listener.Stop();
            }
        }

        [UnityTest]
        public IEnumerator Dispose_FromConnectionErrorCallback_CompletesConnectionWait()
        {
            using var completed = new ManualResetEventSlim();
            using var reserved = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            reserved.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            var handler = new ProbeHandler();
            var channel = NewChannel(handler);
            var handle = HandleOf(channel);
            Exception callbackError = null;
            handler.OnError = (_, _) =>
            {
                try { channel.Dispose(); }
                catch (Exception exception) { callbackError = exception; }
                finally { completed.Set(); }
            };
            bool connected = true;

            yield return Await(channel.ConnectAsync("127.0.0.1", ((IPEndPoint)reserved.LocalEndPoint).Port), result => connected = result);
            yield return Until(() => completed.IsSet, "The connection error callback did not exit.");

            Assert.That(connected, Is.False);
            Assert.That(callbackError, Is.Null);
            Assert.That(handle.IsClosed, Is.True);
            Assert.That(Volatile.Read(ref handler.ErrorCount), Is.EqualTo(1));
        }

        private SocketChannel NewChannel(ProbeHandler handler = null)
        {
            var channel = new SocketChannel(handler ?? new ProbeHandler());
            _channels.Add(channel);
            return channel;
        }

        private static Socket SocketOf(SocketChannel channel) => (Socket)SocketField.GetValue(channel);

        private static bool BuffersReturned(SocketChannel channel)
        {
            return BufferStorageField.GetValue(channel.SendBuffer) == null
                && BufferStorageField.GetValue(ReceiveBufferField.GetValue(channel)) == null;
        }

        private static SafeHandle HandleOf(SocketChannel channel)
        {
            var socket = SocketOf(channel);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var property = typeof(Socket).GetProperty("SafeHandle", flags);
            if (property != null) return (SafeHandle)property.GetValue(socket);
            // Unity Mono exposes m_Handle, while other supported runtimes expose SafeHandle.
            foreach (var field in typeof(Socket).GetFields(flags))
                if (typeof(SafeHandle).IsAssignableFrom(field.FieldType))
                    return (SafeHandle)field.GetValue(socket);
            Assert.Fail("The native socket handle must be observable for this regression.");
            return null;
        }

        private static void Tick() => TickMethod.Invoke(null, null);

        private static IEnumerator Await(ETask<bool> task, Action<bool> completed = null)
        {
            yield return Until(() => task.GetAwaiter().IsCompleted, "The socket operation did not complete within five seconds.");
            bool result = task.GetAwaiter().GetResult();
            completed?.Invoke(result);
            Tick();
            Tick();
        }

        private static IEnumerator Until(Func<bool> predicate, string timeoutMessage)
        {
            var timeout = Stopwatch.StartNew();
            while (!predicate() && timeout.ElapsedMilliseconds < 5000)
            {
                Tick();
                yield return null;
            }
            Assert.That(predicate(), Is.True, timeoutMessage);
        }

        private static TcpListener Listen()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return listener;
        }

        private static int PortOf(TcpListener listener) => ((IPEndPoint)listener.LocalEndpoint).Port;

        private static IEnumerator Connect(SocketChannel channel, TcpListener listener, Action<Socket> accepted)
        {
            var accepting = listener.AcceptSocketAsync();
            bool connected = false;
            yield return Await(channel.ConnectAsync("127.0.0.1", PortOf(listener)), result => connected = result);
            Assert.That(connected, Is.True);
            yield return Until(() => accepting.IsCompleted, "The local listener did not accept the connection.");
            accepted(accepting.GetAwaiter().GetResult());
        }

        private sealed class ProbeHandler : ISocketHandler
        {
            public Action OnReceived;
            public Action<IRingBuffer> ReceiveOverride;
            public Action<ESocketOperation, SocketError> OnError;
            public int ReceiveCount;
            public int ErrorCount;

            public void OnSocketError(ESocketOperation operation, SocketError socketError)
            {
                Interlocked.Increment(ref ErrorCount);
                OnError?.Invoke(operation, socketError);
            }

            public void OnReceive(IRingBuffer buffer)
            {
                Interlocked.Increment(ref ReceiveCount);
                if (ReceiveOverride != null)
                {
                    ReceiveOverride(buffer);
                    return;
                }
                buffer.AdvanceRead(buffer.Length);
                OnReceived?.Invoke();
            }
        }
    }
}
