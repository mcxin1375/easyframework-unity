using System;
using System.IO;
using System.Linq;
using EasyFramework.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace EasyFramework.Tests
{
    public class AssetBundleBuildLayoutTests
    {
        private AssetBundleBuilderSettings _settings;
        private string[] _previousDirectories;
        private BuildAssetBundleOptions _previousOptions;
        private bool _previousCreateManifest;
        private string _assetRoot;
        private string _texturePath;
        private string _materialPath;
        private string _prefabAPath;
        private string _prefabBPath;
        private string _analysisReportPath;
        private string _analysisReportDirectory;
        private bool _analysisReportDirectoryExisted;
        private byte[] _previousAnalysisReport;
        private string[] _publishedBundlesBefore;

        [SetUp]
        public void SetUp()
        {
            _settings = AssetBundleBuilderSettings.Instance;
            _previousDirectories = _settings.buildDirectories;
            _previousOptions = _settings.buildAssetBundleOptions;
            _previousCreateManifest = _settings.createManifestFile;

            _assetRoot = $"Assets/__AssetBundleBuildLayoutTests_{Guid.NewGuid():N}";
            var bundleDirectoryA = _assetRoot + "/BundleA";
            var bundleDirectoryB = _assetRoot + "/BundleB";
            var sharedDirectory = _assetRoot + "/Shared";
            Directory.CreateDirectory(bundleDirectoryA);
            Directory.CreateDirectory(bundleDirectoryB);
            Directory.CreateDirectory(sharedDirectory);

            _texturePath = sharedDirectory + "/SharedTexture.asset";
            _materialPath = sharedDirectory + "/SharedMaterial.mat";
            _prefabAPath = bundleDirectoryA + "/LayoutPrefabA.prefab";
            _prefabBPath = bundleDirectoryB + "/LayoutPrefabB.prefab";
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "SharedTexture"
            };
            texture.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray());
            texture.Apply();
            AssetDatabase.CreateAsset(texture, _texturePath);
            AssetDatabase.SaveAssetIfDirty(texture);

            var shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Sprites/Default");
            Assert.That(shader, Is.Not.Null, "A texture material shader must be available for the test.");
            var material = new Material(shader)
            {
                name = "SharedMaterial",
                mainTexture = texture
            };
            AssetDatabase.CreateAsset(material, _materialPath);
            AssetDatabase.SaveAssetIfDirty(material);

            CreatePrefab(_prefabAPath, material);
            CreatePrefab(_prefabBPath, material);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            _settings.buildDirectories = new[] { bundleDirectoryA, bundleDirectoryB };
            _settings.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            _settings.createManifestFile = false;

            _analysisReportPath = Path.Combine(AssetBundleBuilder.Instance.DebugPlatformPath,
                "AssetBundleBuildLayout.Analysis.json");
            _analysisReportDirectory = Path.GetDirectoryName(_analysisReportPath);
            _analysisReportDirectoryExisted = Directory.Exists(_analysisReportDirectory);
            if (File.Exists(_analysisReportPath))
                _previousAnalysisReport = File.ReadAllBytes(_analysisReportPath);

            _publishedBundlesBefore = GetPublishedBundles();
        }

        [TearDown]
        public void TearDown()
        {
            if (_settings != null)
            {
                _settings.buildDirectories = _previousDirectories;
                _settings.buildAssetBundleOptions = _previousOptions;
                _settings.createManifestFile = _previousCreateManifest;
            }

            if (!string.IsNullOrEmpty(_assetRoot))
                AssetDatabase.DeleteAsset(_assetRoot);

            if (_analysisReportPath != null)
            {
                if (_previousAnalysisReport != null)
                    File.WriteAllBytes(_analysisReportPath, _previousAnalysisReport);
                else if (File.Exists(_analysisReportPath))
                    File.Delete(_analysisReportPath);
            }

            if (!_analysisReportDirectoryExisted && Directory.Exists(_analysisReportDirectory) &&
                !Directory.EnumerateFileSystemEntries(_analysisReportDirectory).Any())
                Directory.Delete(_analysisReportDirectory);
        }

        [Test]
        public void AnalyzeBySettings_ReportsRepeatedTextureObjectsWithoutWritingBundles()
        {
            var report = AssetBundleBuilderUtility.AnalyzeBySettings();

            Assert.That(report, Is.Not.Null);
            Assert.That(report.isSimulation, Is.True);
            Assert.That(report.buildTarget, Is.EqualTo(EditorUserBuildSettings.activeBuildTarget.ToString()));

            var textureEntries = report.bundles
                .SelectMany(bundle => bundle.resources
                    .Where(resource => string.Equals(resource.assetPath, _texturePath,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(resource => new { bundle.name, resource }))
                .ToArray();

            Assert.That(textureEntries, Has.Length.EqualTo(2));
            Assert.That(textureEntries.Select(entry => entry.name).Distinct().Count(), Is.EqualTo(2));
            Assert.That(textureEntries.All(entry => entry.resource.isDuplicate), Is.True);
            Assert.That(textureEntries.All(entry => entry.resource.duplicateObjectCount > 0), Is.True);
            Assert.That(textureEntries.All(entry => entry.resource.objectIds.Length > 0), Is.True);
            Assert.That(textureEntries.All(entry => entry.resource.referencingFiles.Length > 0), Is.True);
            Assert.That(File.Exists(_analysisReportPath), Is.True);
            CollectionAssert.AreEqual(_publishedBundlesBefore, GetPublishedBundles(),
                "Layout analysis must not write or replace published AssetBundles.");
        }

        private static void CreatePrefab(string path, Material material)
        {
            var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                instance.GetComponent<MeshRenderer>().sharedMaterial = material;
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
                AssetDatabase.SaveAssetIfDirty(prefab);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static string[] GetPublishedBundles()
        {
            var outputPath = AssetBundleBuilder.Instance.ProjectPlatformPath;
            if (!Directory.Exists(outputPath)) return Array.Empty<string>();

            return Directory.GetFiles(outputPath, "*.ab", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
