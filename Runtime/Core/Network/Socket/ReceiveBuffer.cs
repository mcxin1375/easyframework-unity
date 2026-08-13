// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.IO;
//
// namespace EasyFramework
// {
//
//     public interface IReceiveBuffer
//     {
//         long Length { get; }
//         int Read(byte[] bytes, int offset, int count);
//         void Read(Stream stream, int count);
//     }
//
//     public class ReceiveBuffer : Stream, IReceiveBuffer
//     {
//         public int ChunkSize = 8192;
//
//         private readonly Queue<byte[]> _bufferQueue = new Queue<byte[]>();
//         private readonly Queue<byte[]> _bufferCache = new Queue<byte[]>();
//
//         private int _chunkLastIndex;
//         public int ChunkLastIndex
//         {
//             get => _chunkLastIndex;
//             set
//             {
//                 _chunkLastIndex = value;
//                 if (_chunkLastIndex == ChunkSize)
//                 {
//                     AddLast();
//                     _chunkLastIndex = 0;
//                 }
//             }
//         }
//
//         public byte[] ChunkBuff
//         {
//             get
//             {
//                 if (_bufferQueue.Count == 0)
//                 {
//                     AddLast();
//                 }
//                 return _lastBuffer;
//             }
//         }
//
//         public override long Length
//         {
//             get
//             {
//                 if (_bufferQueue.Count == 0) return 0;
//                 
//                 int c = (_bufferQueue.Count - 1) * ChunkSize + ChunkLastIndex - FirstIndex;
//                 if (c < 0)
//                 {
//                     // Log.Warning("ReceiveBuffer count < 0:", _bufferQueue.Count, ChunkLastIndex, FirstIndex);
//                 }
//                 return c;
//             }
//         }
//
//         private int FirstIndex { get; set; }
//         private byte[] _lastBuffer;
//
//         public ReceiveBuffer()
//         {
//             AddLast();
//         }
//
//         public void Reset()
//         {
//             while (_bufferQueue.Count > 0)
//             {
//                 _bufferCache.Enqueue(_bufferQueue.Dequeue());
//             }
//             
//             _lastBuffer = _bufferCache.Dequeue();
//             _bufferQueue.Enqueue(_lastBuffer);
//             FirstIndex = 0;
//             _chunkLastIndex = 0;
//         }
//
//         private void AddLast()
//         {
//             byte[] buffer;
//             if (_bufferCache.Count > 0)
//             {
//                 buffer = _bufferCache.Dequeue();
//             }
//             else
//             {
//                 buffer = new byte[ChunkSize];
//             }
//             _bufferQueue.Enqueue(buffer);
//             _lastBuffer = buffer;
//         }
//
//         private void RemoveFirst()
//         {
//             _bufferCache.Enqueue(_bufferQueue.Dequeue());
//         }
//
//         private byte[] First
//         {
//             get
//             {
//                 if (_bufferQueue.Count == 0)
//                 {
//                     AddLast();
//                 }
//                 return _bufferQueue.Peek();
//             }
//         }
//
//         public void Read(Stream stream, int count)
//         {
//             if (count > this.Length)
//             {
//                 //throw new Exception($"bufferList length < count, {Length} {count}");
//             }
//
//             int alreadyCopyCount = 0;
//             while (alreadyCopyCount < count)
//             {
//                 int n = count - alreadyCopyCount;
//                 if (ChunkSize - this.FirstIndex > n)
//                 {
//                     stream.Write(this.First, this.FirstIndex, n);
//                     this.FirstIndex += n;
//                     alreadyCopyCount += n;
//                 }
//                 else
//                 {
//                     stream.Write(this.First, this.FirstIndex, ChunkSize - this.FirstIndex);
//                     alreadyCopyCount += ChunkSize - this.FirstIndex;
//                     this.FirstIndex = 0;
//                     this.RemoveFirst();
//                 }
//             }
//         }
//
//         // 从stream写入CircularBuffer
//         public void Write(Stream stream)
//         {
//             int count = (int)(stream.Length - stream.Position);
//
//             int alreadyCopyCount = 0;
//             while (alreadyCopyCount < count)
//             {
//                 if (this.ChunkLastIndex == ChunkSize)
//                 {
//                     this.AddLast();
//                     this.ChunkLastIndex = 0;
//                 }
//
//                 int n = count - alreadyCopyCount;
//                 if (ChunkSize - this.ChunkLastIndex > n)
//                 {
//                     stream.Read(this._lastBuffer, this.ChunkLastIndex, n);
//                     this.ChunkLastIndex += count - alreadyCopyCount;
//                     alreadyCopyCount += n;
//                 }
//                 else
//                 {
//                     stream.Read(this._lastBuffer, this.ChunkLastIndex, ChunkSize - this.ChunkLastIndex);
//                     alreadyCopyCount += ChunkSize - this.ChunkLastIndex;
//                     this.ChunkLastIndex = ChunkSize;
//                 }
//             }
//         }
//
//         public override int Read(byte[] buffer, int offset, int count)
//         {
//             if (buffer.Length < offset + count)
//             {
//                 //throw new Exception($"bufferList length < coutn, buffer length: {buffer.Length} {offset} {count}");
//             }
//
//             long length = this.Length;
//             if (length < count)
//             {
//                 count = (int)length;
//             }
//
//             int alreadyCopyCount = 0;
//             while (alreadyCopyCount < count)
//             {
//                 int n = count - alreadyCopyCount;
//                 if (ChunkSize - this.FirstIndex > n)
//                 {
//                     Array.Copy(this.First, this.FirstIndex, buffer, alreadyCopyCount + offset, n);
//                     this.FirstIndex += n;
//                     alreadyCopyCount += n;
//                 }
//                 else
//                 {
//                     Array.Copy(this.First, this.FirstIndex, buffer, alreadyCopyCount + offset, ChunkSize - this.FirstIndex);
//                     alreadyCopyCount += ChunkSize - this.FirstIndex;
//                     this.FirstIndex = 0;
//                     this.RemoveFirst();
//                 }
//             }
//
//             return count;
//         }
//
//         public override void Write(byte[] buffer, int offset, int count)
//         {
//             int alreadyCopyCount = 0;
//             while (alreadyCopyCount < count)
//             {
//                 if (this.ChunkLastIndex == ChunkSize)
//                 {
//                     this.AddLast();
//                     this.ChunkLastIndex = 0;
//                 }
//
//                 int n = count - alreadyCopyCount;
//                 if (ChunkSize - this.ChunkLastIndex > n)
//                 {
//                     Array.Copy(buffer, alreadyCopyCount + offset, this._lastBuffer, this.ChunkLastIndex, n);
//                     this.ChunkLastIndex += count - alreadyCopyCount;
//                     alreadyCopyCount += n;
//                 }
//                 else
//                 {
//                     Array.Copy(buffer, alreadyCopyCount + offset, this._lastBuffer, this.ChunkLastIndex, ChunkSize - this.ChunkLastIndex);
//                     alreadyCopyCount += ChunkSize - this.ChunkLastIndex;
//                     this.ChunkLastIndex = ChunkSize;
//                 }
//             }
//         }
//
//         public override void Flush()
//         {
//             throw new NotImplementedException();
//         }
//
//         public override long Seek(long offset, SeekOrigin origin)
//         {
//             throw new NotImplementedException();
//         }
//
//         public override void SetLength(long value)
//         {
//             throw new NotImplementedException();
//         }
//
//         public override bool CanRead => true;
//         public override bool CanSeek => false;
//         public override bool CanWrite => true;
//         public override long Position { get; set; }
//     }
// }