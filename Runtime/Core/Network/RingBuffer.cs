/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/2
// describe:
//----------------------------------------------------------------*/

using System;
using System.Buffers;

namespace EasyFramework
{
    public class RingBuffer : IRingBuffer
    {
        private byte[] _buffer;
        private byte[] _tmpBuffer;

        // SPSC 指针
        private volatile int _readPos;
        private volatile int _writePos;

        // 扩容锁（仅扩容时使用）
        private readonly object _resizeLock = new();

        public int Capacity => _buffer.Length;

        public int Length
        {
            get
            {
                int w = _writePos;
                int r = _readPos;
                return w >= r ? w - r : w + Capacity - r;
            }
        }

        public int FreeLength => Capacity - Length;
        
        public RingBuffer(int capacity = 1024 * 128)
        {
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
        }
        
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacityForWrite(sizeHint);
            
            int tailFree = Capacity - _writePos;
            int writable = sizeHint > 0 && tailFree >= sizeHint ? sizeHint : tailFree;
            
            return _buffer.AsSpan(_writePos, writable);
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacityForWrite(sizeHint);

            int tailFree = Capacity - _writePos;
            int writable = sizeHint > 0 && tailFree >= sizeHint ? sizeHint : tailFree;
            
            return _buffer.AsMemory(_writePos, writable);
        }
        
        public ArraySegment<byte> GetSegment(int sizeHint = 0)
        {
            EnsureCapacityForWrite(sizeHint);
            
            int tailFree = Capacity - _writePos;
            int writable = sizeHint > 0 && tailFree >= sizeHint ? sizeHint : tailFree;

            return new ArraySegment<byte>(_buffer, _writePos, writable);
        }
        
        public void GetWritableSpans(out Span<byte> first, out Span<byte> second)
        {
            GetSpan(FreeLength > 0 ? FreeLength : Capacity, out first, out second);
        }
        public void GetSpan(int length, out Span<byte> first, out Span<byte> second)
        {
            EnsureCapacityForWrite(length);

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
            if (count < 0 || count > FreeLength)
                throw new ArgumentOutOfRangeException(nameof(count));

            _writePos = (_writePos + count) % Capacity;
        }
        public void AdvanceRead(int count)
        {
            if (count < 0 || count > Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            _readPos = (_readPos + count) % Capacity;
            
            if (_tmpBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_tmpBuffer);
                _tmpBuffer = null;
            }
        }
        
        public void Compact()
        {
            if (_readPos <= _writePos) return;

            int currentLength = Length;

            var newBuffer = ArrayPool<byte>.Shared.Rent(Capacity);

            int firstPart = Capacity - _readPos;

            Buffer.BlockCopy(_buffer, _readPos, newBuffer, 0, firstPart);
            Buffer.BlockCopy(_buffer, 0, newBuffer, firstPart, _writePos);

            ArrayPool<byte>.Shared.Return(_buffer);

            _buffer = newBuffer;
            _readPos = 0;
            _writePos = currentLength;
        }
        public void Clear()
        {
            _readPos = 0;
            _writePos = 0;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
            if (_tmpBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_tmpBuffer);
                _tmpBuffer = null;
            }
        }

        public void EnsureCapacityForWrite(int appendLength)
        {
            if (appendLength <= FreeLength)
                return;

            lock (_resizeLock)
            {
                // 再检查一次（双检锁）
                if (appendLength <= FreeLength)
                    return;

                int currentLength = Length;
                int required = currentLength + appendLength;
                int newSize = Math.Max(Capacity * 2, required);

                var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);

                // 拷贝旧数据（保持顺序）
                if (currentLength > 0)
                {
                    int r = _readPos;
                    int w = _writePos;

                    if (r < w)
                    {
                        Buffer.BlockCopy(_buffer, r, newBuffer, 0, currentLength);
                    }
                    else
                    {
                        int firstPart = Capacity - r;
                        Buffer.BlockCopy(_buffer, r, newBuffer, 0, firstPart);
                        Buffer.BlockCopy(_buffer, 0, newBuffer, firstPart, w);
                    }
                }

                ArrayPool<byte>.Shared.Return(_buffer);

                _buffer = newBuffer;
                _readPos = 0;
                _writePos = currentLength;
            }
        }
    }
}