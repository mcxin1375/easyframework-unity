using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class RingBufferTests
    {
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void Write_CapacityBoundaryPreservesEveryByte(int offset)
        {
            using var buffer = new RingBuffer(16);
            var expected = Bytes(buffer.Capacity + offset);
            buffer.Write(expected);
            AssertContents(buffer, expected);
        }

        [Test]
        public void Write_WrappedExactCapacityPreservesOrder()
        {
            using var buffer = new RingBuffer(16);
            int capacity = buffer.Capacity;
            var initial = Bytes(capacity - 3);
            buffer.Write(initial);
            buffer.AdvanceRead(capacity - 6);
            var appended = Bytes(capacity - 3, 100);
            buffer.Write(appended);
            var expected = new List<byte>(initial.AsSpan(capacity - 6).ToArray());
            expected.AddRange(appended);
            AssertContents(buffer, expected.ToArray());
        }

        [TestCase(0, -1)]
        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(1, -1)]
        [TestCase(1, 0)]
        [TestCase(1, 1)]
        [TestCase(2, -1)]
        [TestCase(2, 0)]
        [TestCase(2, 1)]
        public void DirectWrite_CapacityBoundaryPreservesEveryByte(int api, int offset)
        {
            using var buffer = new RingBuffer(16);
            var expected = Bytes(buffer.Capacity + offset);
            Fill(buffer, expected, api, expected.Length);
            AssertContents(buffer, expected);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void DirectWrite_WrappedSizeHintReturnsContiguousFreeMemory(int api)
        {
            using var buffer = new RingBuffer(16);
            int capacity = buffer.Capacity;
            var initial = Bytes(capacity - 2);
            buffer.Write(initial);
            buffer.AdvanceRead(capacity - 5);
            var appended = Bytes(6, 100);
            Fill(buffer, appended, api, appended.Length);
            var expected = new List<byte>(initial.AsSpan(capacity - 5).ToArray());
            expected.AddRange(appended);
            AssertContents(buffer, expected.ToArray());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void DirectWrite_ZeroHintCannotExposeUnreadBytes(int api)
        {
            using var buffer = new RingBuffer(16);
            int capacity = buffer.Capacity;
            buffer.Write(Bytes(capacity - 2));
            buffer.AdvanceRead(capacity - 5);
            buffer.Write(Bytes(4, 90));
            byte[] expected = Peek(buffer);
            int free = buffer.FreeLength;
            int writable = WritableLength(buffer, api, 0);
            Assert.That(writable, Is.GreaterThan(0).And.LessThanOrEqualTo(free));
            AssertContents(buffer, expected);
        }

        [Test]
        public void SplitWrite_WrappedExactCapacityPreservesOrder()
        {
            using var buffer = new RingBuffer(16);
            int capacity = buffer.Capacity;
            buffer.Write(Bytes(capacity - 2));
            buffer.AdvanceRead(capacity - 5);
            byte[] remaining = Peek(buffer);
            var appended = Bytes(capacity - remaining.Length, 100);
            buffer.GetSpan(appended.Length, out var first, out var second);
            appended.AsSpan(0, first.Length).CopyTo(first);
            appended.AsSpan(first.Length).CopyTo(second);
            buffer.Advance(appended.Length);
            var expected = new List<byte>(remaining);
            expected.AddRange(appended);
            AssertContents(buffer, expected.ToArray());
        }

        [Test]
        public void GetWritableSpans_FullBufferGrowsAndPreservesData()
        {
            using var buffer = new RingBuffer(16);
            byte[] initial = Bytes(buffer.Capacity);
            buffer.Write(initial);
            buffer.GetWritableSpans(out var first, out var second);
            Assert.That(first.Length + second.Length, Is.GreaterThan(0));
            first[0] = 251;
            buffer.Advance(1);
            var expected = new List<byte>(initial) { 251 };
            AssertContents(buffer, expected.ToArray());
        }

        [Test]
        public void Compact_FullWrappedBufferAndReadToEmptyPreserveInvariants()
        {
            using var buffer = new RingBuffer(16);
            int capacity = buffer.Capacity;
            buffer.Write(Bytes(capacity));
            buffer.AdvanceRead(5);
            buffer.Write(Bytes(5, 100));
            byte[] expected = Peek(buffer);
            buffer.Compact();
            AssertContents(buffer, expected);
            buffer.AdvanceRead(expected.Length);
            AssertContents(buffer, Array.Empty<byte>());
            buffer.Write(Bytes(capacity));
            buffer.Clear();
            AssertContents(buffer, Array.Empty<byte>());
        }

        [Test]
        public void Growth_WrappedDataStaysInOrder()
        {
            using var buffer = new RingBuffer(16);
            int capacity = buffer.Capacity;
            buffer.Write(Bytes(capacity - 2));
            buffer.AdvanceRead(capacity - 5);
            buffer.Write(Bytes(4, 100));
            var expected = new List<byte>(Peek(buffer));
            byte[] appended = Bytes(capacity + 1, 170);
            expected.AddRange(appended);
            buffer.Write(appended);
            AssertContents(buffer, expected.ToArray());
        }

        [Test]
        public void Advance_RejectsBytesBeyondReturnedMemory()
        {
            using var buffer = new RingBuffer(16);
            int writable = buffer.GetMemory(1).Length;
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Advance(writable + 1));
            Assert.That(buffer.Length, Is.Zero);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Advance_RejectsInvalidatedBorrow(int operation)
        {
            using var buffer = new RingBuffer(16);
            buffer.GetMemory(8);
            switch (operation)
            {
                case 0: buffer.GetMemory(1); break;
                case 1: buffer.Clear(); break;
                case 2: buffer.EnsureCapacityForWrite(buffer.Capacity + 1); break;
                default: buffer.Write(new byte[] { 100 }); break;
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Advance(8));
            AssertContents(buffer, operation == 3 ? new byte[] { 100 } : Array.Empty<byte>());
        }

        [Test]
        public void NegativeLengthsAreRejectedWithoutChangingData()
        {
            using var buffer = new RingBuffer(16);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetMemory(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetSegment(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.EnsureCapacityForWrite(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Advance(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AdvanceRead(-1));
            Assert.That(buffer.Length, Is.Zero);
        }

        [Test]
        public void MixedOperations_MatchReferenceQueue()
        {
            using var buffer = new RingBuffer(16);
            var expected = new List<byte>();
            var random = new Random(20261102);
            for (int i = 0; i < 1000; i++)
            {
                if (expected.Count > 100 || random.Next(3) == 0)
                {
                    int count = random.Next(expected.Count + 1);
                    buffer.AdvanceRead(count);
                    expected.RemoveRange(0, count);
                }
                else
                {
                    byte[] bytes = Bytes(random.Next(1, 20), i);
                    if (random.Next(2) == 0) buffer.Write(bytes);
                    else Fill(buffer, bytes, random.Next(3), bytes.Length);
                    expected.AddRange(bytes);
                }
                if (i % 17 == 0) buffer.Compact();
                AssertContents(buffer, expected.ToArray());
            }
        }

        private static byte[] Bytes(int count, int start = 0)
        {
            var result = new byte[count];
            for (int i = 0; i < count; i++) result[i] = (byte)(start + i);
            return result;
        }

        private static void Fill(RingBuffer buffer, byte[] bytes, int api, int hint)
        {
            switch (api)
            {
                case 0:
                    var memory = buffer.GetMemory(hint);
                    Assert.That(memory.Length, Is.GreaterThanOrEqualTo(bytes.Length));
                    bytes.AsSpan().CopyTo(memory.Span);
                    break;
                case 1:
                    var span = buffer.GetSpan(hint);
                    Assert.That(span.Length, Is.GreaterThanOrEqualTo(bytes.Length));
                    bytes.AsSpan().CopyTo(span);
                    break;
                default:
                    var segment = buffer.GetSegment(hint);
                    Assert.That(segment.Count, Is.GreaterThanOrEqualTo(bytes.Length));
                    bytes.AsSpan().CopyTo(segment.AsSpan());
                    break;
            }
            buffer.Advance(bytes.Length);
        }

        private static int WritableLength(RingBuffer buffer, int api, int hint)
        {
            switch (api)
            {
                case 0: return buffer.GetMemory(hint).Length;
                case 1: return buffer.GetSpan(hint).Length;
                default: return buffer.GetSegment(hint).Count;
            }
        }

        private static byte[] Peek(RingBuffer buffer)
        {
            Assert.That(buffer.TryPeek(buffer.Length, out var bytes), Is.True);
            return bytes.ToArray();
        }

        private static void AssertContents(RingBuffer buffer, byte[] expected)
        {
            Assert.That(buffer.Length, Is.EqualTo(expected.Length));
            Assert.That(buffer.FreeLength, Is.EqualTo(buffer.Capacity - expected.Length));
            CollectionAssert.AreEqual(expected, Peek(buffer));
            Assert.That(buffer.TryPeek(expected.Length, out var first, out var second), Is.True);
            var split = new List<byte>(first.ToArray());
            split.AddRange(second.ToArray());
            CollectionAssert.AreEqual(expected, split);
            if (expected.Length == 0) return;
            Assert.That(buffer.TryPeekSegment(expected.Length, out var segment), Is.True);
            CollectionAssert.AreEqual(expected.AsSpan(0, segment.Count).ToArray(), segment);
        }
    }
}
