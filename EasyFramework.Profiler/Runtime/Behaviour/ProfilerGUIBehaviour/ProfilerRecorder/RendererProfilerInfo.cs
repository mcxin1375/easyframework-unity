using System;

namespace EasyFramework.Profiler
{
    [Serializable]
    public class RendererProfilerInfo
    {
        public long BatchesCount;
        public long SetPassCallsCount;
        public long DrawCallsCount;
        public long TrianglesCount;
        public long VerticesCount;
        public long RenderTexturesCount;
        public long RenderTexturesBytes;
        public long RenderTexturesChangesCount;
        public long UsedBuffersCount;
        public long UsedBuffersBytes;
        public long VertexBufferUploadInFrameCount;
        public long VertexBufferUploadInFrameBytes;
        public long IndexBufferUploadInFrameCount;
        public long IndexBufferUploadInFrameBytes;
        public long ShadowCastersCount;
        public long UsedTexturesCount;
        public long UsedTexturesBytes;
        public long DynamicBatchedDrawCallsCount;
        public long DynamicBatchesCount;
        public long DynamicBatchedTrianglesCount;
        public long DynamicBatchedVerticesCount;
        public long DynamicBatchingTime;
        public long StaticBatchedDrawCallsCount;
        public long StaticBatchesCount;
        public long StaticBatchedTrianglesCount;
        public long StaticBatchedVerticesCount;
        public long InstancedBatchedDrawCallsCount;
        public long InstancedBatchesCount;
        public long InstancedBatchedTrianglesCount;
        public long InstancedBatchedVerticesCount;

    }
}
