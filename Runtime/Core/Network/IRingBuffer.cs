/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/2
// describe:
//----------------------------------------------------------------*/

using System;
using System.Buffers;

namespace EasyFramework
{
    public interface IRingBuffer : IDisposable, IBufferWriter<byte>
    {
        int Length { get; }
        int FreeLength { get; }
        int Capacity { get; }
        
        void Write(ReadOnlySpan<byte> span);
        
        // Base: IBufferWriter<byte>
        // void Advance(int count);
        // Span<byte> GetSpan(int sizeHint = 0);
        // Memory<byte> GetMemory(int sizeHint = 0)
        ArraySegment<byte> GetSegment(int sizeHint = 0);
        void GetSpan(int length, out Span<byte> first, out Span<byte> second);
        void GetWritableSpans(out Span<byte> first, out Span<byte> second);
        void EnsureCapacityForWrite(int appendLength);

        bool TryPeek(int length, out ReadOnlySpan<byte> buffer);
        bool TryPeekSegment(int length, out ArraySegment<byte> segment);
        void AdvanceRead(int count);

        void Compact();
        void Clear();
    }
}