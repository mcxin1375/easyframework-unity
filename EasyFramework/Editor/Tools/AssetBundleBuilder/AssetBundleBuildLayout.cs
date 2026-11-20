using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

using BuildGuid = UnityEditor.GUID;

namespace EasyFramework.Editor
{
    [Serializable]
    public sealed class AssetBundleBuildLayoutReport
    {
        public int schemaVersion = 2;
        public string buildTarget;
        public string generatedAt;
        public int buildOptions;
        public bool isSimulation;
        public bool hasContentAnalysis;
        public AssetBundleBuildLayout[] bundles = Array.Empty<AssetBundleBuildLayout>();
    }

    [Serializable]
    public sealed class AssetBundleBuildLayout
    {
        public string name;
        public AssetBundleBuildLayoutResource[] resources = Array.Empty<AssetBundleBuildLayoutResource>();
    }

    [Serializable]
    public sealed class AssetBundleBuildLayoutResource
    {
        public string assetPath;
        public bool isDirect;
        public bool isDuplicate;
        public int duplicateObjectCount;
        public long duplicateObjectBytes;
        public bool isContentDuplicate;
        public int contentDuplicateObjectCount;
        public long contentDuplicateBytes;
        public string[] duplicateBundles = Array.Empty<string>();
        public string[] contentDuplicateBundles = Array.Empty<string>();
        public string[] contentHashes = Array.Empty<string>();
        public long serializedSize;
        public string[] objectIds = Array.Empty<string>();
        public string[] referencingFiles = Array.Empty<string>();
    }

    public sealed class AssetBundleBuildLayoutCapture : IBuildTask
    {
        private readonly string _buildTarget;
        private readonly BuildAssetBundleOptions _buildOptions;
        private readonly bool _isSimulation;
        private readonly bool _captureSerializedContent;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildContent _buildContent;

        [InjectContext(ContextUsage.In)]
        private IDependencyData _dependencyData;

        [InjectContext]
        private IBundleWriteData _writeData;

        [InjectContext]
        private IBundleBuildResults _buildResults;

        public int Version => 2;
        public AssetBundleBuildLayoutReport Report { get; private set; }

        public AssetBundleBuildLayoutCapture(string buildTarget, BuildAssetBundleOptions buildOptions,
            bool isSimulation, bool captureSerializedContent)
        {
            _buildTarget = buildTarget;
            _buildOptions = buildOptions;
            _isSimulation = isSimulation;
            _captureSerializedContent = captureSerializedContent;
        }

        public ReturnCode Run()
        {
            var bundles = new Dictionary<string, BundleData>(StringComparer.OrdinalIgnoreCase);

            foreach (var bundleLayout in _buildContent.BundleLayout)
            {
                var bundle = new BundleData(bundleLayout.Key);
                bundles.Add(bundle.Name, bundle);

                foreach (var rootGuid in bundleLayout.Value)
                {
                    var rootPath = NormalizeAssetPath(AssetDatabase.GUIDToAssetPath(rootGuid.ToString()));
                    if (string.IsNullOrWhiteSpace(rootPath)) continue;

                    bundle.DirectPaths.Add(rootPath);
                    GetOrAddResource(bundle, rootPath).IsDirect = true;

                    if (_dependencyData.AssetInfo.TryGetValue(rootGuid, out var assetInfo))
                    {
                        foreach (var dependency in assetInfo.referencedObjects)
                            AddReference(bundle, dependency.guid, rootPath);
                    }
                    else if (_dependencyData.SceneInfo.TryGetValue(rootGuid, out var sceneInfo))
                    {
                        foreach (var dependency in sceneInfo.referencedObjects)
                            AddReference(bundle, dependency.guid, rootPath);
                    }
                }
            }

            foreach (var file in _writeData.FileToObjects)
            {
                if (!_writeData.FileToBundle.TryGetValue(file.Key, out var bundleName) ||
                    !bundles.TryGetValue(bundleName, out var bundle)) continue;

                foreach (var objectIdentifier in file.Value)
                {
                    var assetPath = GetAssetPath(objectIdentifier);
                    if (string.IsNullOrWhiteSpace(assetPath)) continue;

                    var resource = GetOrAddResource(bundle, assetPath);
                    resource.ObjectIds.Add(GetObjectKey(objectIdentifier));
                    if (bundle.DirectPaths.Contains(assetPath)) resource.IsDirect = true;
                    if (bundle.ReferencingFiles.TryGetValue(objectIdentifier.guid, out var references))
                        resource.ReferencingFiles.UnionWith(references);
                }
            }

            if (_captureSerializedContent)
                CaptureSerializedObjects(bundles);

            MarkDuplicateObjects(bundles.Values);
            if (_captureSerializedContent)
                MarkDuplicateContent(bundles.Values);

            Report = new AssetBundleBuildLayoutReport
            {
                buildTarget = _buildTarget,
                generatedAt = DateTime.UtcNow.ToString("o"),
                buildOptions = (int)_buildOptions,
                isSimulation = _isSimulation,
                hasContentAnalysis = _captureSerializedContent && _buildResults?.WriteResults?.Count > 0,
                bundles = bundles.Values
                    .OrderBy(bundle => bundle.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(bundle => new AssetBundleBuildLayout
                    {
                        name = bundle.Name,
                        resources = bundle.Resources.Values
                            .OrderBy(resource => resource.AssetPath, StringComparer.OrdinalIgnoreCase)
                            .Select(resource => new AssetBundleBuildLayoutResource
                            {
                                assetPath = resource.AssetPath,
                                isDirect = resource.IsDirect,
                                isDuplicate = resource.DuplicateObjectCount > 0,
                                duplicateObjectCount = resource.DuplicateObjectCount,
                                duplicateObjectBytes = resource.DuplicateObjectBytes,
                                isContentDuplicate = resource.ContentDuplicateObjectCount > 0,
                                contentDuplicateObjectCount = resource.ContentDuplicateObjectCount,
                                contentDuplicateBytes = resource.ContentDuplicateBytes,
                                duplicateBundles = resource.DuplicateBundles
                                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray(),
                                contentDuplicateBundles = resource.ContentDuplicateBundles
                                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray(),
                                contentHashes = resource.Objects.Values
                                    .Where(data => !string.IsNullOrEmpty(data.ContentHash))
                                    .Select(data => data.ContentHash)
                                    .Distinct(StringComparer.Ordinal)
                                    .OrderBy(hash => hash, StringComparer.Ordinal)
                                    .ToArray(),
                                serializedSize = resource.Objects.Values.Sum(data => data.SerializedSize),
                                objectIds = resource.ObjectIds.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                                referencingFiles = resource.ReferencingFiles
                                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                                    .ToArray()
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            return ReturnCode.Success;
        }

        private static void MarkDuplicateObjects(IEnumerable<BundleData> bundles)
        {
            var objectBundles = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (var bundle in bundles)
            {
                foreach (var resource in bundle.Resources.Values)
                {
                    foreach (var objectId in resource.ObjectIds)
                    {
                        var key = GetResourceObjectKey(resource.AssetPath, objectId);
                        if (!objectBundles.TryGetValue(key, out var bundleNames))
                        {
                            bundleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            objectBundles.Add(key, bundleNames);
                        }

                        bundleNames.Add(bundle.Name);
                    }
                }
            }

            foreach (var bundle in bundles)
            {
                foreach (var resource in bundle.Resources.Values)
                {
                    foreach (var objectId in resource.ObjectIds)
                    {
                        var bundleNames = objectBundles[GetResourceObjectKey(resource.AssetPath, objectId)];
                        if (bundleNames.Count <= 1) continue;

                        resource.DuplicateObjectCount++;
                        resource.DuplicateBundles.UnionWith(bundleNames);
                        if (resource.Objects.TryGetValue(objectId, out var objectData))
                        {
                            objectData.IsDuplicateObject = true;
                            objectData.DuplicateBundles.UnionWith(bundleNames);
                            var canonicalBundle = bundleNames.OrderBy(name => name,
                                StringComparer.OrdinalIgnoreCase).First();
                            if (!string.Equals(bundle.Name, canonicalBundle, StringComparison.OrdinalIgnoreCase))
                                resource.DuplicateObjectBytes += objectData.SerializedSize;
                        }
                    }
                }
            }
        }

        private void CaptureSerializedObjects(Dictionary<string, BundleData> bundles)
        {
            if (_buildResults?.WriteResults == null) return;

            foreach (var pair in _buildResults.WriteResults)
            {
                if (!_writeData.FileToBundle.TryGetValue(pair.Key, out var bundleName) ||
                    !bundles.TryGetValue(bundleName, out var bundle)) continue;

                var serializedObjects = pair.Value.serializedObjects;
                if (serializedObjects == null) continue;

                foreach (var serializedObject in serializedObjects)
                {
                    var objectIdentifier = serializedObject.serializedObject;
                    var assetPath = GetAssetPath(objectIdentifier);
                    if (string.IsNullOrWhiteSpace(assetPath)) continue;

                    var objectId = GetObjectKey(objectIdentifier);
                    var resource = GetOrAddResource(bundle, assetPath);
                    resource.ObjectIds.Add(objectId);
                    if (bundle.DirectPaths.Contains(assetPath)) resource.IsDirect = true;

                    var objectData = resource.GetOrAddObject(objectId);
                    objectData.SerializedSize = GetSerializedSize(serializedObject);
                    objectData.ContentHash = CalculateContentHash(serializedObject);
                }
            }
        }

        private static void MarkDuplicateContent(IEnumerable<BundleData> bundles)
        {
            var contentGroups = new Dictionary<string, List<ObjectOccurrence>>(StringComparer.Ordinal);
            foreach (var bundle in bundles)
            {
                foreach (var resource in bundle.Resources.Values)
                {
                    foreach (var pair in resource.Objects)
                    {
                        if (string.IsNullOrEmpty(pair.Value.ContentHash) || pair.Value.SerializedSize <= 0) continue;
                        if (!contentGroups.TryGetValue(pair.Value.ContentHash, out var occurrences))
                        {
                            occurrences = new List<ObjectOccurrence>();
                            contentGroups.Add(pair.Value.ContentHash, occurrences);
                        }

                        occurrences.Add(new ObjectOccurrence(bundle.Name, resource, pair.Value));
                    }
                }
            }

            foreach (var occurrences in contentGroups.Values)
            {
                var bundleNames = occurrences.Select(item => item.BundleName)
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                var objectIds = occurrences.Select(item => item.ObjectData.ObjectId)
                    .Distinct(StringComparer.Ordinal).ToArray();
                if (bundleNames.Length <= 1 || objectIds.Length <= 1) continue;

                var ordered = occurrences.OrderBy(item => item.BundleName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(item => item.Resource.AssetPath, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(item => item.ObjectData.ObjectId, StringComparer.Ordinal)
                    .ToList();
                for (var index = 0; index < ordered.Count; index++)
                {
                    var occurrence = ordered[index];
                    occurrence.Resource.ContentDuplicateObjectCount++;
                    occurrence.Resource.ContentDuplicateBundles.UnionWith(bundleNames);
                    occurrence.ObjectData.IsContentDuplicate = true;
                    occurrence.ObjectData.ContentDuplicateBundles.UnionWith(bundleNames);
                    if (index > 0) occurrence.Resource.ContentDuplicateBytes += occurrence.ObjectData.SerializedSize;
                }
            }
        }

        private static long GetSerializedSize(ObjectSerializedInfo serializedObject)
        {
            return (long)serializedObject.header.size + (long)serializedObject.rawData.size;
        }

        private static string CalculateContentHash(ObjectSerializedInfo serializedObject)
        {
            if (serializedObject.header.size == 0 && serializedObject.rawData.size == 0) return null;

            using (var hash = SHA256.Create())
            {
                if (!AppendLocation(hash, serializedObject.header) ||
                    !AppendLocation(hash, serializedObject.rawData)) return null;
                hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return BitConverter.ToString(hash.Hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static bool AppendLocation(HashAlgorithm hash, SerializedLocation location)
        {
            if (location.size == 0) return true;
            if (string.IsNullOrEmpty(location.fileName) || !File.Exists(location.fileName)) return false;

            using (var stream = File.OpenRead(location.fileName))
            {
                stream.Position = (long)location.offset;
                var buffer = new byte[64 * 1024];
                long remaining = (long)location.size;
                while (remaining > 0)
                {
                    var read = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                    if (read <= 0) break;
                    hash.TransformBlock(buffer, 0, read, buffer, 0);
                    remaining -= read;
                }

                return remaining == 0;
            }
        }

        private static string GetResourceObjectKey(string assetPath, string objectId)
        {
            return $"{assetPath}\0{objectId}";
        }

        private static void AddReference(BundleData bundle, BuildGuid assetGuid, string rootPath)
        {
            if (!bundle.ReferencingFiles.TryGetValue(assetGuid, out var references))
            {
                references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                bundle.ReferencingFiles.Add(assetGuid, references);
            }

            references.Add(rootPath);
        }

        private static AssetBundleBuildLayoutResourceData GetOrAddResource(BundleData bundle, string assetPath)
        {
            if (!bundle.Resources.TryGetValue(assetPath, out var resource))
            {
                resource = new AssetBundleBuildLayoutResourceData(assetPath);
                bundle.Resources.Add(assetPath, resource);
            }

            return resource;
        }

        private static string GetAssetPath(ObjectIdentifier objectIdentifier)
        {
            var path = AssetDatabase.GUIDToAssetPath(objectIdentifier.guid.ToString());
            var normalizedPath = NormalizeAssetPath(string.IsNullOrWhiteSpace(path)
                ? objectIdentifier.filePath
                : path);
            return IsGeneratedAssetPath(normalizedPath) ? null : normalizedPath;
        }

        private static bool IsGeneratedAssetPath(string path)
        {
            return path != null &&
                   (path.StartsWith("temp:/", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("archive:/", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("library:/", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetObjectKey(ObjectIdentifier objectIdentifier)
        {
            return $"{objectIdentifier.guid}:{objectIdentifier.localIdentifierInFile}";
        }

        private static string NormalizeAssetPath(string path)
        {
            return path?.Replace('\\', '/');
        }

        private sealed class BundleData
        {
            public readonly string Name;
            public readonly HashSet<string> DirectPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<BuildGuid, HashSet<string>> ReferencingFiles =
                new Dictionary<BuildGuid, HashSet<string>>();
            public readonly Dictionary<string, AssetBundleBuildLayoutResourceData> Resources =
                new Dictionary<string, AssetBundleBuildLayoutResourceData>(StringComparer.OrdinalIgnoreCase);

            public BundleData(string name)
            {
                Name = name;
            }
        }

        private sealed class AssetBundleBuildLayoutResourceData
        {
            public readonly string AssetPath;
            public readonly HashSet<string> ObjectIds = new HashSet<string>(StringComparer.Ordinal);
            public readonly HashSet<string> ReferencingFiles =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SerializedObjectData> Objects =
                new Dictionary<string, SerializedObjectData>(StringComparer.Ordinal);
            public readonly HashSet<string> DuplicateBundles =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public readonly HashSet<string> ContentDuplicateBundles =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public int DuplicateObjectCount;
            public long DuplicateObjectBytes;
            public int ContentDuplicateObjectCount;
            public long ContentDuplicateBytes;
            public bool IsDirect;

            public AssetBundleBuildLayoutResourceData(string assetPath)
            {
                AssetPath = assetPath;
            }

            public SerializedObjectData GetOrAddObject(string objectId)
            {
                if (!Objects.TryGetValue(objectId, out var objectData))
                {
                    objectData = new SerializedObjectData(objectId);
                    Objects.Add(objectId, objectData);
                }

                return objectData;
            }
        }

        private sealed class SerializedObjectData
        {
            public readonly string ObjectId;
            public string ContentHash;
            public long SerializedSize;
            public bool IsDuplicateObject;
            public bool IsContentDuplicate;
            public readonly HashSet<string> DuplicateBundles =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public readonly HashSet<string> ContentDuplicateBundles =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public SerializedObjectData(string objectId)
            {
                ObjectId = objectId;
            }
        }

        private sealed class ObjectOccurrence
        {
            public readonly string BundleName;
            public readonly AssetBundleBuildLayoutResourceData Resource;
            public readonly SerializedObjectData ObjectData;

            public ObjectOccurrence(string bundleName, AssetBundleBuildLayoutResourceData resource,
                SerializedObjectData objectData)
            {
                BundleName = bundleName;
                Resource = resource;
                ObjectData = objectData;
            }
        }
    }
}
