/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/3/2
// describe:
//----------------------------------------------------------------*/

using System;
using System.Buffers;

namespace EasyFramework
{
    /// <summary>
    /// 可使用全部 Capacity 的环形字节缓冲区，Length 为已提交且未消费的字节数。
    /// GetSpan/GetMemory/GetSegment 返回至少 sizeHint 的连续可写区间，零提示也返回非空区间；
    /// 双 Span 重载可分段返回。填充后通过 Advance 提交，提交量不能超过本次返回的区间。
    /// 再次获取写区间、Write、扩容、Compact、Clear 或 Dispose 会使已有借用失效。
    /// 不提供跨线程借用/扩容/释放的同步，调用者必须协调并发访问与借用内存的生命周期。
    /// </summary>
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
