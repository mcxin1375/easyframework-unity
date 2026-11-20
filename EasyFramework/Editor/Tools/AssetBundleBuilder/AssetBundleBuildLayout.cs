using System;
using System.Collections.Generic;
using System.Linq;
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
        public int schemaVersion = 1;
        public string buildTarget;
        public string generatedAt;
        public int buildOptions;
        public bool isSimulation;
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
        public string[] objectIds = Array.Empty<string>();
        public string[] referencingFiles = Array.Empty<string>();
    }

    public sealed class AssetBundleBuildLayoutCapture : IBuildTask
    {
        private readonly string _buildTarget;
        private readonly BuildAssetBundleOptions _buildOptions;
        private readonly bool _isSimulation;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildContent _buildContent;

        [InjectContext(ContextUsage.In)]
        private IDependencyData _dependencyData;

        [InjectContext]
        private IBundleWriteData _writeData;

        public int Version => 1;
        public AssetBundleBuildLayoutReport Report { get; private set; }

        public AssetBundleBuildLayoutCapture(string buildTarget, BuildAssetBundleOptions buildOptions,
            bool isSimulation)
        {
            _buildTarget = buildTarget;
            _buildOptions = buildOptions;
            _isSimulation = isSimulation;
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

            MarkDuplicateObjects(bundles.Values);

            Report = new AssetBundleBuildLayoutReport
            {
                buildTarget = _buildTarget,
                generatedAt = DateTime.UtcNow.ToString("o"),
                buildOptions = (int)_buildOptions,
                isSimulation = _isSimulation,
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
                    resource.DuplicateObjectCount = resource.ObjectIds.Count(objectId =>
                        objectBundles[GetResourceObjectKey(resource.AssetPath, objectId)].Count > 1);
                }
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
            return NormalizeAssetPath(string.IsNullOrWhiteSpace(path) ? objectIdentifier.filePath : path);
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
            public int DuplicateObjectCount;
            public bool IsDirect;

            public AssetBundleBuildLayoutResourceData(string assetPath)
            {
                AssetPath = assetPath;
            }
        }
    }
}
