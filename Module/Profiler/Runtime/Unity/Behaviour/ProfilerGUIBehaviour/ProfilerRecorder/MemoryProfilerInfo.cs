using System;

namespace EasyFramework.Profiler
{
    [Serializable]
    public class MemoryProfilerInfo
    {
        public long TotalUsedMemory;
        public long TotalReservedMemory;
        public long GCUsedMemory;
        public long GCReservedMemory;
        public long AudioUsedMemory;
        public long AudioReservedMemory;
        public long VideoUsedMemory;
        public long VideoReservedMemory;
        public long ProfilerUsedMemory;
        public long ProfilerReservedMemory;
        public long SystemUsedMemory;
        public long GfxUsedMemory;
        public long GfxReservedMemory;
        public long TextureCount;
        public long TextureMemory;
        public long MeshCount;
        public long MeshMemory;
        public long MaterialCount;
        public long MaterialMemory;
        public long AnimationClipCount;
        public long AnimationClipMemory;
        public long AssetCount;
        public long GameObjectCount;
        public long SceneObjectCount;
        public long ObjectCount;
        public long GCAllocationInFrameCount;
        public long GCAllocatedInFrame;

    }
}
