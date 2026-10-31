/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/25
// describe??
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace EasyFramework.Profiler
{
    public static class FloatEx
    {
        public static string FormatNumber(this int num) => FormatHelper.FormatNumber(num);
        public static string FormatNumber(this long num) => FormatHelper.FormatNumber(num);
        public static string FormatByte(this int num) => FormatHelper.FormatByte(num);
        public static string FormatByte(this long num) => FormatHelper.FormatByte(num);
    }

    public class ProfilerRecorderEx
    {
        // ProfilerCategory.Internal
        
        [RecorderInfo(RecorderInfoAttribute.Category.Internal, "Main Thread")] 
        public long MainThread { get; private set; }
        
        // ProfilerCategory.Memory
        
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Total Used Memory")]
        public long TotalUsedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Total Reserved Memory")]
        public long TotalReservedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "GC Used Memory")]
        public long GCUsedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "GC Reserved Memory")]
        public long GCReservedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Audio Used Memory")]
        public long AudioUsedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Audio Reserved Memory")]
        public long AudioReservedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Video Used Memory")]
        public long VideoUsedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Video Reserved Memory")]
        public long VideoReservedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Profiler Used Memory")]
        public long ProfilerUsedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Profiler Reserved Memory")]
        public long ProfilerReservedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "System Used Memory")]
        public long SystemUsedMemory { get; private set; }

        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Gfx Used Memory", true)]
        public long GfxUsedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Gfx Reserved Memory", true)]
        public long GfxReservedMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Texture Count", true)]
        public long TextureCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Texture Memory", true)]
        public long TextureMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Mesh Count", true)]
        public long MeshCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Mesh Memory", true)]
        public long MeshMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Material Count", true)]
        public long MaterialCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Material Memory", true)]
        public long MaterialMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "AnimationClip Count", true)]
        public long AnimationClipCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "AnimationClip Memory", true)]
        public long AnimationClipMemory { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Asset Count", true)]
        public long AssetCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "GameObject Count", true)]
        public long GameObjectCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Scene Object Count", true)]
        public long SceneObjectCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "Object Count", true)]
        public long ObjectCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "GC Allocation In Frame Count", true)]
        public long GCAllocationInFrameCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Memory, "GC Allocated In Frame", true)]
        public long GCAllocatedInFrame { get; private set; }
        
        // ProfilerCategory.Render
        
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Batches Count")]
        public long BatchesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "SetPass Calls Count")]
        public long SetPassCallsCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Draw Calls Count")]
        public long DrawCallsCount { get; private set; }
        // public const string TotalBatchesCount = "Total Batches Count";
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Triangles Count")]
        public long TrianglesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Vertices Count")]
        public long VerticesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Render Textures Count")]
        public long RenderTexturesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Render Textures Bytes")]
        public long RenderTexturesBytes { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Render Textures Changes Count")]
        public long RenderTexturesChangesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Used Buffers Count")]
        public long UsedBuffersCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Used Buffers Bytes")]
        public long UsedBuffersBytes { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Vertex Buffer Upload In Frame Count")]
        public long VertexBufferUploadInFrameCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Vertex Buffer Upload In Frame Bytes")]
        public long VertexBufferUploadInFrameBytes { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Index Buffer Upload In Frame Count")]
        public long IndexBufferUploadInFrameCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Index Buffer Upload In Frame Bytes")]
        public long IndexBufferUploadInFrameBytes { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Shadow Casters Count")]
        public long ShadowCastersCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Used Textures Count")]
        public long UsedTexturesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Used Textures Bytes")]
        public long UsedTexturesBytes { get; private set; }
        
        // (Dynamic Batching)
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Dynamic Batched Draw Calls Count")]
        public long DynamicBatchedDrawCallsCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Dynamic Batches Count")]
        public long DynamicBatchesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Dynamic Batched Triangles Count")]
        public long DynamicBatchedTrianglesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Dynamic Batched Vertices Count")]
        public long DynamicBatchedVerticesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Dynamic Batching Time")]
        public long DynamicBatchingTime { get; private set; }
        
        // (Static Batching)
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Static Batched Draw Calls Count")]
        public long StaticBatchedDrawCallsCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Static Batches Count")]
        public long StaticBatchesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Static Batched Triangles Count")]
        public long StaticBatchedTrianglesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Static Batched Vertices Count")]
        public long StaticBatchedVerticesCount { get; private set; }
            
        // (Instancing)
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Instanced Batched Draw Calls Count")]
        public long InstancedBatchedDrawCallsCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Instanced Batches Count")]
        public long InstancedBatchesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Instanced Batched Triangles Count")]
        public long InstancedBatchedTrianglesCount { get; private set; }
        [RecorderInfo(RecorderInfoAttribute.Category.Render, "Instanced Batched Vertices Count")]
        public long InstancedBatchedVerticesCount { get; private set; }
        
        
        private readonly Dictionary<string, ProfilerRecorder> _profilerRecorderDict = new();
        private PropertyInfo[] _propertyInfos;

        public ProfilerRecorderEx()
        {
            _propertyInfos = GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in _propertyInfos)
            {
                var recorderInfo = propertyInfo.GetCustomAttribute<RecorderInfoAttribute>();
                if (recorderInfo == null) continue;
                if (recorderInfo.EditorOnly && !Application.isEditor) continue;
                
                if (!_profilerRecorderDict.ContainsKey(recorderInfo.StatName))
                {
                    _profilerRecorderDict.Add(propertyInfo.Name, ProfilerRecorder.StartNew(recorderInfo.ProfilerCategory, recorderInfo.StatName));
                }
            }
        }

        public void Dispose()
        {
            foreach (ProfilerRecorder recorder in _profilerRecorderDict.Values)
            {
                recorder.Dispose();
            }
        }

        public void OnUpdate()
        {
            if (_propertyInfos?.Length > 0)
            {
                foreach (PropertyInfo propertyInfo in _propertyInfos)
                {
                    var lastValue = _profilerRecorderDict.ContainsKey(propertyInfo.Name) ? _profilerRecorderDict[propertyInfo.Name].LastValue : 0;
                    if (lastValue == 0) continue;
                    
                    propertyInfo.SetValue(this, lastValue);
                }
            }
        }

        public string MemoryToString()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append($"MainThread: {MainThread/1000000:N1}ms\n");
            sb.Append($"TotalMemory(Used/Reserve): {TotalUsedMemory.FormatByte()} / {TotalReservedMemory.FormatByte()}\n");
            // string color = TotalUsedMemory > setting.TotalUsedMemory ? "red" : "white";
            // string color1 = TotalReservedMemory > setting.TotalReservedMemory ? "red" : "white";
            sb.Append($"TotalMemory(Used/Reserve): {TotalUsedMemory.FormatByte()} / {TotalReservedMemory.FormatByte()}\n");
            sb.Append($"GC: {GCUsedMemory.FormatByte()} / {GCReservedMemory.FormatByte()}\n");
            sb.Append($"Audio: {AudioUsedMemory.FormatByte()} / {AudioReservedMemory.FormatByte()}\n");
            sb.Append($"Video: {VideoUsedMemory.FormatByte()} / {VideoReservedMemory.FormatByte()}\n");
            sb.Append($"Profiler: {ProfilerUsedMemory.FormatByte()} / {ProfilerReservedMemory.FormatByte()}\n");
            if (Application.isEditor)
            {
                sb.Append($"Gfx: {FormatHelper.FormatByte(GfxUsedMemory)} / {FormatHelper.FormatByte(GfxReservedMemory)}\n");
            }
            sb.Append($"SystemUsedMemory: {SystemUsedMemory.FormatByte()}\n\n");

            if (Application.isEditor)
            {
                sb.Append($"Textures: {TextureCount.FormatNumber()} / {FormatHelper.FormatByte(TextureMemory)}\n");
                sb.Append($"Meshes: {MeshCount.FormatNumber()} / {FormatHelper.FormatByte(MeshMemory)}\n");
                sb.Append($"Materials: {MaterialCount.FormatNumber()} / {FormatHelper.FormatByte(MaterialMemory)}\n");
                sb.Append($"AnimationClips: {AnimationClipCount.FormatNumber()} / {FormatHelper.FormatByte(AnimationClipMemory)}\n");
                sb.Append($"AssetCount: {AssetCount.FormatNumber()}\n");
                sb.Append($"GameObjectCount: {GameObjectCount.FormatNumber()}\n");
                sb.Append($"SceneObjectCount: {SceneObjectCount.FormatNumber()}\n");
                sb.Append($"ObjectCount: {ObjectCount.FormatNumber()}\n\n");
                sb.Append($"GC Allocated In Frame: {GCAllocationInFrameCount.FormatNumber()} / {FormatHelper.FormatByte(GCAllocatedInFrame)}\n");
            }
            return sb.ToString();
        }

        public string RenderToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"SetPass Calls: {SetPassCallsCount.FormatNumber()}\n");
            sb.Append($"Draw Calls: {DrawCallsCount.FormatNumber()}\n");
            sb.Append($"Batches: {BatchesCount.FormatNumber()}\n");
            sb.Append($"Tris: {TrianglesCount.FormatNumber()}\n");
            sb.Append($"Verts: {VerticesCount.FormatNumber()}\n\n");
            
            sb.Append($"Render Textures: {RenderTexturesCount.FormatNumber()} / {FormatHelper.FormatByte(RenderTexturesBytes)}\n");
            sb.Append($"RenderTexturesChanges: {RenderTexturesChangesCount.FormatNumber()}\n");
            sb.Append($"UsedBuffers: {UsedBuffersCount.FormatNumber()} / {FormatHelper.FormatByte(UsedBuffersBytes)}\n");
            sb.Append($"VertexBufferUploadInFrame: {VertexBufferUploadInFrameCount.FormatNumber()} / {FormatHelper.FormatByte(VertexBufferUploadInFrameBytes)}\n");
            sb.Append($"IndexBufferUploadInFrame: {IndexBufferUploadInFrameCount.FormatNumber()} / {FormatHelper.FormatByte(IndexBufferUploadInFrameBytes)}\n");
            if (Application.isEditor)
            {
                sb.Append($"UsedTextures: {UsedTexturesCount.FormatNumber()} / {FormatHelper.FormatByte(UsedTexturesBytes)}\n");
            }
            sb.Append($"ShadowCasters: {ShadowCastersCount.FormatNumber()}\n\n");
            
            if (Application.isEditor)
            {
                sb.Append($"(Dynamic)\nDC: {DynamicBatchedDrawCallsCount.FormatNumber()} Batches: {DynamicBatchesCount.FormatNumber()} Tris: {DynamicBatchedTrianglesCount.FormatNumber()} Verts: {DynamicBatchedVerticesCount.FormatNumber()} Time: {DynamicBatchingTime}\n");
                sb.Append($"(Static)\nDC: {StaticBatchedDrawCallsCount.FormatNumber()} Batches: {StaticBatchesCount.FormatNumber()} Tris: {StaticBatchedTrianglesCount.FormatNumber()} Verts: {StaticBatchedVerticesCount.FormatNumber()}\n");
                sb.Append($"(Instancing)\nDC: {InstancedBatchedDrawCallsCount.FormatNumber()} Batches: {InstancedBatchesCount.FormatNumber()} Tris: {InstancedBatchedTrianglesCount.FormatNumber()} Verts: {InstancedBatchedVerticesCount.FormatNumber()}\n");
            }
            return sb.ToString();
        }
    }
}