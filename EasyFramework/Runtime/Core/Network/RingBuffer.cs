/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/2
// describe:
//----------------------------------------------------------------*/

using System;
using System.Buffers;
using System.Threading;

namespace EasyFramework
{
    public class RingBuffer : IRingBuffer
    {
        private byte[] _buffer;
        private byte[] _tmpBuffer;

        // 满和空都允许读写指针重合，由有效字节数区分。
        private volatile int _readPos;
        private volatile int _writePos;
        private int _length;
        private int _writableLength;

        // 扩容锁（仅扩容时使用）
        private readonly object _resizeLock = new();

        public int Capacity => _buffer.Length;

        public int Length => Volatile.Read(ref _length);

        public int FreeLength => Capacity - Length;
        
        public RingBuffer(int capacity = 1024 * 128)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _buffer = ArrayPool<byte>.Shared.Rent(capacity);
        }
        
        public void Write(ReadOnlySpan<byte> span)
        {
            int len = span.Length;

            EnsureCapacityForWrite(len);

            int tail = Capacity - _writePos;

            if (tail >= len)
            {
                span.CopyTo(_buffer.AsSpan(_writePos, len));
            }
            else
            {
                span[..tail].CopyTo(_buffer.AsSpan(_writePos, tail));
                span[tail..].CopyTo(_buffer.AsSpan(0, len - tail));
            }

            _writePos = (_writePos + len) % Capacity;
            _writableLength = 0;
            Interlocked.Add(ref _length, len);
        }
        
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            int writable = PrepareContiguousWrite(sizeHint);
            return _buffer.AsSpan(_writePos, writable);
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            int writable = PrepareContiguousWrite(sizeHint);
            return _buffer.AsMemory(_writePos, writable);
        }
        
        public ArraySegment<byte> GetSegment(int sizeHint = 0)
        {
            int writable = PrepareContiguousWrite(sizeHint);
            return new ArraySegment<byte>(_buffer, _writePos, writable);
        }
        
        public void GetWritableSpans(out Span<byte> first, out Span<byte> second)
        {
            GetSpan(FreeLength > 0 ? FreeLength : Capacity, out first, out second);
        }
        public void GetSpan(int length, out Span<byte> first, out Span<byte> second)
        {
            EnsureCapacityForWrite(length);
            _writableLength = length;

            int tailFree = Capacity - _writePos;

            if (tailFree >= length)
            {
                first = _buffer.AsSpan(_writePos, length);
                second = default;
            }
            else
            {
                first = _buffer.AsSpan(_writePos, tailFree);
                second = _buffer.AsSpan(0, length - tailFree);
            }
        }
        
        public bool TryPeek(int length, out ReadOnlySpan<byte> buffer)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length > Length)
            {
                buffer = default;
                return false;
            }

            int tailAvailable = Capacity - _readPos;
            if (tailAvailable >= length)
            {
                buffer = _buffer.AsSpan(_readPos, length);
            }
            else
            {
                if (_tmpBuffer == null)
                {
                    _tmpBuffer = ArrayPool<byte>.Shared.Rent(length);
                }
                else if(_tmpBuffer.Length < length)
                {
                    ArrayPool<byte>.Shared.Return(_tmpBuffer);
                    _tmpBuffer = ArrayPool<byte>.Shared.Rent(length);
                }
                
                _buffer.AsSpan(_readPos, tailAvailable).CopyTo(_tmpBuffer);
                _buffer.AsSpan(0, length - tailAvailable).CopyTo(_tmpBuffer.AsSpan(tailAvailable));

                buffer = _tmpBuffer.AsSpan(0, length);
            }

            return true;
        }
        public bool TryPeek(int length, out ReadOnlySpan<byte> first, out ReadOnlySpan<byte> second)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length > Length)
            {
                first = default;
                second = default;
                return false;
            }

            int tailAvailable = Capacity - _readPos;
            if (tailAvailable >= length)
            {
                first = _buffer.AsSpan(_readPos, length);
                second = default;
            }
            else
            {
                first = _buffer.AsSpan(_readPos, tailAvailable);
                second = _buffer.AsSpan(0, length - tailAvailable);
            }

            return true;
        }
        
        public bool TryPeekSegment(int length, out ArraySegment<byte> segment)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            int available = Length;
            if (available == 0)
            {
                segment = default;
                return false;
            }

            // 不能超过当前可读总量
            int requested = Math.Min(length, available);
            int tailAvailable = Capacity - _readPos;

            if (_readPos < _writePos)
            {
                // 数据连续
                int count = Math.Min(requested, _writePos - _readPos);
                segment = new ArraySegment<byte>(_buffer, _readPos, count);
            }
            else
            {
                // 数据发生环绕，只能返回尾部这一段
                int count = Math.Min(requested, tailAvailable);
                segment = new ArraySegment<byte>(_buffer, _readPos, count);
            }

            return segment.Count > 0;
        }

        public void Advance(int count)
        {
            if (count < 0 || count > _writableLength)
                throw new ArgumentOutOfRangeException(nameof(count));

            _writePos = (_writePos + count) % Capacity;
            _writableLength = 0;
            Interlocked.Add(ref _length, count);
        }
        public void AdvanceRead(int count)
        {
            if (count < 0 || count > Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            _readPos = (_readPos + count) % Capacity;
            Interlocked.Add(ref _length, -count);
            
            if (_tmpBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_tmpBuffer);
                _tmpBuffer = null;
            }
        }
        
        public void Compact()
        {
            if (_readPos == 0) return;
            MoveToNewBuffer(Capacity);
        }
        public void Clear()
        {
            _readPos = 0;
            _writePos = 0;
            _writableLength = 0;
            Volatile.Write(ref _length, 0);
        }

        public void Dispose()
        {
            if (_buffer == null) return;
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
            _writableLength = 0;
            if (_tmpBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_tmpBuffer);
                _tmpBuffer = null;
            }
        }

        public void EnsureCapacityForWrite(int appendLength)
        {
            if (appendLength < 0) throw new ArgumentOutOfRangeException(nameof(appendLength));
            if (appendLength <= FreeLength)
                return;

            lock (_resizeLock)
            {
                // 再检查一次（双检锁）
                if (appendLength <= FreeLength)
                    return;

                int required = checked(Length + appendLength);
                int doubled = Capacity <= int.MaxValue / 2 ? Capacity * 2 : int.MaxValue;
                MoveToNewBuffer(Math.Max(doubled, required));
            }
        }

        private int PrepareContiguousWrite(int sizeHint)
        {
            if (sizeHint < 0) throw new ArgumentOutOfRangeException(nameof(sizeHint));
            int required = Math.Max(1, sizeHint);
            EnsureCapacityForWrite(required);

            int contiguous = Math.Min(Capacity - _writePos, FreeLength);
            if (contiguous < required)
            {
                // 空间足够但分成两段，线性化后满足 IBufferWriter 的连续区间约定。
                MoveToNewBuffer(Capacity);
                contiguous = Capacity - _writePos;
            }

            _writableLength = sizeHint > 0 ? sizeHint : contiguous;
            return _writableLength;
        }

        private void MoveToNewBuffer(int capacity)
        {
            int length = Length;
            var newBuffer = ArrayPool<byte>.Shared.Rent(capacity);
            int firstPart = Math.Min(length, Capacity - _readPos);
            Buffer.BlockCopy(_buffer, _readPos, newBuffer, 0, firstPart);
            Buffer.BlockCopy(_buffer, 0, newBuffer, firstPart, length - firstPart);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
            _readPos = 0;
            _writePos = length % Capacity;
            _writableLength = 0;
        }
    }
}
