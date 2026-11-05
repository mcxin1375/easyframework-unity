/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/25
// describe??
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Profiling;
using Unity.Profiling.LowLevel;
using Unity.Profiling.LowLevel.Unsafe;
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
        private readonly Dictionary<string, RecorderBinding> _bindings = new();

        public ProfilerRecorderEx()
        {
            HashSet<(ProfilerCategory, string)> releaseCounters = null;
            if (!Application.isEditor && !Debug.isDebugBuild)
            {
                releaseCounters = new HashSet<(ProfilerCategory, string)>();
                var handles = new List<ProfilerRecorderHandle>();
                ProfilerRecorderHandle.GetAvailable(handles);
                foreach (var handle in handles)
                {
                    var description = ProfilerRecorderHandle.GetDescription(handle);
                    if ((description.Flags & MarkerFlags.AvailabilityNonDevelopment) != 0)
                        releaseCounters.Add((description.Category, description.Name));
                }
            }

            foreach (PropertyInfo propertyInfo in GetType().GetProperties())
            {
                var recorderInfo = propertyInfo.GetCustomAttribute<RecorderInfoAttribute>();
                if (recorderInfo == null) continue;
                if (recorderInfo.EditorOnly && !Application.isEditor) continue;
                if (releaseCounters != null &&
                    !releaseCounters.Contains((recorderInfo.ProfilerCategory, recorderInfo.StatName))) continue;
                
                _profilerRecorderDict.Add(propertyInfo.Name, ProfilerRecorder.StartNew(recorderInfo.ProfilerCategory, recorderInfo.StatName));
                _bindings.Add(propertyInfo.Name, new RecorderBinding(
                    (Func<long>)Delegate.CreateDelegate(typeof(Func<long>), this, propertyInfo.GetGetMethod()),
                    (Action<long>)Delegate.CreateDelegate(typeof(Action<long>), this, propertyInfo.GetSetMethod(true)),
                    recorderInfo.ProfilerCategory == ProfilerCategory.Render));
            }
        }

        public void Dispose()
        {
            foreach (ProfilerRecorder recorder in _profilerRecorderDict.Values)
            {
                recorder.Dispose();
            }
            _profilerRecorderDict.Clear();
            foreach (var binding in _bindings.Values) binding.SetValue(0);
        }

        public void OnUpdate() => ReadCounters(false);

        // Per-frame render counters are reset before Update on some Editor frames.
        // Read their current values only after rendering has actually completed.
        public void OnRender() => ReadCounters(true);

        private void ReadCounters(bool rendered)
        {
            foreach (var entry in _bindings)
            {
                var binding = entry.Value;
                if (binding.IsRender != rendered) continue;
                long value = _profilerRecorderDict.TryGetValue(entry.Key, out var recorder) && recorder.Valid
                    ? (rendered ? recorder.CurrentValue : recorder.LastValue)
                    : 0;
                binding.SetValue(value);
            }
        }

        public bool IsAvailable(string propertyName)
        {
            return _profilerRecorderDict.TryGetValue(propertyName, out var recorder) && recorder.Valid;
        }

        public string FormatNumber(string propertyName)
        {
            return IsAvailable(propertyName) ? GetValue(propertyName).FormatNumber() : "N/A";
        }

        public string FormatByte(string propertyName)
        {
            return IsAvailable(propertyName) ? GetValue(propertyName).FormatByte() : "N/A";
        }

        private long GetValue(string propertyName)
        {
            return _bindings[propertyName].GetValue();
        }

        private readonly struct RecorderBinding
        {
            public readonly Func<long> GetValue;
            public readonly Action<long> SetValue;
            public readonly bool IsRender;

            public RecorderBinding(Func<long> getValue, Action<long> setValue, bool isRender)
            {
                GetValue = getValue;
                SetValue = setValue;
                IsRender = isRender;
            }
        }

        public string MemoryToString()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append($"MainThread: {(IsAvailable(nameof(MainThread)) ? $"{MainThread/1000000:N1}ms" : "N/A")}\n");
            sb.Append($"TotalMemory(Used/Reserve): {FormatByte(nameof(TotalUsedMemory))} / {FormatByte(nameof(TotalReservedMemory))}\n");
            // string color = TotalUsedMemory > setting.TotalUsedMemory ? "red" : "white";
            // string color1 = TotalReservedMemory > setting.TotalReservedMemory ? "red" : "white";
            sb.Append($"TotalMemory(Used/Reserve): {FormatByte(nameof(TotalUsedMemory))} / {FormatByte(nameof(TotalReservedMemory))}\n");
            sb.Append($"GC: {FormatByte(nameof(GCUsedMemory))} / {FormatByte(nameof(GCReservedMemory))}\n");
            sb.Append($"Audio: {FormatByte(nameof(AudioUsedMemory))} / {FormatByte(nameof(AudioReservedMemory))}\n");
            sb.Append($"Video: {FormatByte(nameof(VideoUsedMemory))} / {FormatByte(nameof(VideoReservedMemory))}\n");
            sb.Append($"Profiler: {FormatByte(nameof(ProfilerUsedMemory))} / {FormatByte(nameof(ProfilerReservedMemory))}\n");
            if (Application.isEditor)
            {
                sb.Append($"Gfx: {FormatByte(nameof(GfxUsedMemory))} / {FormatByte(nameof(GfxReservedMemory))}\n");
            }
            sb.Append($"SystemUsedMemory: {FormatByte(nameof(SystemUsedMemory))}\n\n");

            if (Application.isEditor)
            {
                sb.Append($"Textures: {FormatNumber(nameof(TextureCount))} / {FormatByte(nameof(TextureMemory))}\n");
                sb.Append($"Meshes: {FormatNumber(nameof(MeshCount))} / {FormatByte(nameof(MeshMemory))}\n");
                sb.Append($"Materials: {FormatNumber(nameof(MaterialCount))} / {FormatByte(nameof(MaterialMemory))}\n");
                sb.Append($"AnimationClips: {FormatNumber(nameof(AnimationClipCount))} / {FormatByte(nameof(AnimationClipMemory))}\n");
                sb.Append($"AssetCount: {FormatNumber(nameof(AssetCount))}\n");
                sb.Append($"GameObjectCount: {FormatNumber(nameof(GameObjectCount))}\n");
                sb.Append($"SceneObjectCount: {FormatNumber(nameof(SceneObjectCount))}\n");
                sb.Append($"ObjectCount: {FormatNumber(nameof(ObjectCount))}\n\n");
                sb.Append($"GC Allocated In Frame: {FormatNumber(nameof(GCAllocationInFrameCount))} / {FormatByte(nameof(GCAllocatedInFrame))}\n");
            }
            return sb.ToString();
        }

        public string RenderToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"SetPass Calls: {FormatNumber(nameof(SetPassCallsCount))}\n");
            sb.Append($"Draw Calls: {FormatNumber(nameof(DrawCallsCount))}\n");
            sb.Append($"Batches: {FormatNumber(nameof(BatchesCount))}\n");
            sb.Append($"Tris: {FormatNumber(nameof(TrianglesCount))}\n");
            sb.Append($"Verts: {FormatNumber(nameof(VerticesCount))}\n\n");
            
            sb.Append($"Render Textures: {FormatNumber(nameof(RenderTexturesCount))} / {FormatByte(nameof(RenderTexturesBytes))}\n");
            sb.Append($"RenderTexturesChanges: {FormatNumber(nameof(RenderTexturesChangesCount))}\n");
            sb.Append($"UsedBuffers: {FormatNumber(nameof(UsedBuffersCount))} / {FormatByte(nameof(UsedBuffersBytes))}\n");
            sb.Append($"VertexBufferUploadInFrame: {FormatNumber(nameof(VertexBufferUploadInFrameCount))} / {FormatByte(nameof(VertexBufferUploadInFrameBytes))}\n");
            sb.Append($"IndexBufferUploadInFrame: {FormatNumber(nameof(IndexBufferUploadInFrameCount))} / {FormatByte(nameof(IndexBufferUploadInFrameBytes))}\n");
            if (Application.isEditor)
            {
                sb.Append($"UsedTextures: {FormatNumber(nameof(UsedTexturesCount))} / {FormatByte(nameof(UsedTexturesBytes))}\n");
            }
            sb.Append($"ShadowCasters: {FormatNumber(nameof(ShadowCastersCount))}\n\n");
            
            if (Application.isEditor)
            {
                sb.Append($"(Dynamic)\nDC: {FormatNumber(nameof(DynamicBatchedDrawCallsCount))} Batches: {FormatNumber(nameof(DynamicBatchesCount))} Tris: {FormatNumber(nameof(DynamicBatchedTrianglesCount))} Verts: {FormatNumber(nameof(DynamicBatchedVerticesCount))} Time: {(IsAvailable(nameof(DynamicBatchingTime)) ? DynamicBatchingTime.ToString() : "N/A")}\n");
                sb.Append($"(Static)\nDC: {FormatNumber(nameof(StaticBatchedDrawCallsCount))} Batches: {FormatNumber(nameof(StaticBatchesCount))} Tris: {FormatNumber(nameof(StaticBatchedTrianglesCount))} Verts: {FormatNumber(nameof(StaticBatchedVerticesCount))}\n");
                sb.Append($"(Instancing)\nDC: {FormatNumber(nameof(InstancedBatchedDrawCallsCount))} Batches: {FormatNumber(nameof(InstancedBatchesCount))} Tris: {FormatNumber(nameof(InstancedBatchedTrianglesCount))} Verts: {FormatNumber(nameof(InstancedBatchedVerticesCount))}\n");
            }
            return sb.ToString();
        }
    }
}
