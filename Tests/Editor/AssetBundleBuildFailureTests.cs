using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EasyFramework.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;

namespace EasyFramework.Tests
{
    public class AssetBundleBuildFailureTests
    {
        private string _root;
        private string _output;
        private string[] _directories;
        private BuildAssetBundleOptions _options;
        private bool _createManifest;
        private string _assetRoot;

        [SetUp]
        public void SetUp()
        {
            var settings = AssetBundleBuilderSettings.Instance;
            _directories = settings.buildDirectories;
            _options = settings.buildAssetBundleOptions;
            _createManifest = settings.createManifestFile;
            settings.createManifestFile = false;
            _root = Path.GetFullPath($"Library/Order5Validation/ABFailure-{Guid.NewGuid():N}");
            _output = Path.Combine(_root, "Output");
            Directory.CreateDirectory(_output);
            File.WriteAllText(Path.Combine(_output, "old.ab"), "previous usable bundle");
            File.WriteAllText(Path.Combine(_output, EasyFramework.AssetBundleManifest.FileName), "previous manifest");
        }

        [TearDown]
        public void TearDown()
        {
            var settings = AssetBundleBuilderSettings.Instance;
            settings.buildDirectories = _directories;
            settings.buildAssetBundleOptions = _options;
            settings.createManifestFile = _createManifest;
            if (_assetRoot != null) AssetDatabase.DeleteAsset(_assetRoot);
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Test]
        public void InvalidBuildInput_ThrowsAndPreservesPreviousOutput()
        {
            var source = Path.Combine(_root, "NotAUnityAsset");
            Directory.CreateDirectory(source);
            File.WriteAllText(Path.Combine(source, "InvalidAsset.txt"), "outside the AssetDatabase");
            AssetBundleBuilderSettings.Instance.buildDirectories = new[] { source };
            AssetBundleBuilderSettings.Instance.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;

            Assert.Throws<ArgumentException>(() => AssetBundleBuilderUtility.BuildBySettings(_output));

            AssertPreviousOutput();
            Assert.That(Directory.GetDirectories(_root), Has.Length.EqualTo(2), "The failed build must remove its staging directory.");
        }

        [Test]
        public void DryRunBuild_CannotPublishOrReplacePreviousOutput()
        {
            AssetBundleBuilderSettings.Instance.buildAssetBundleOptions = BuildAssetBundleOptions.DryRunBuild;

            Assert.Throws<BuildFailedException>(() => AssetBundleBuilderUtility.BuildBySettings(_output));

            AssertPreviousOutput();
            Assert.That(Directory.GetDirectories(_root), Has.Length.EqualTo(1));
        }

        [Test]
        public void SuccessfulBuild_ThenSettingsChange_ReplacesOldBundlesWithCurrentConfiguration()
        {
            _assetRoot = "Assets/__EasyFrameworkABSuccess_" + Guid.NewGuid().ToString("N");
            var directoryA = _assetRoot + "/A";
            var directoryB = _assetRoot + "/B";
            Directory.CreateDirectory(directoryA);
            Directory.CreateDirectory(directoryB);
            File.WriteAllText(directoryA + "/order5_first.txt", "first bundle");
            File.WriteAllText(directoryB + "/order5_second.txt", "second bundle");
            AssetDatabase.Refresh();
            var settings = AssetBundleBuilderSettings.Instance;
            settings.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            settings.buildDirectories = new[] { directoryA };
            AssetBundleBuilderUtility.BuildBySettings(_output);
            Assert.That(File.Exists(Path.Combine(_output, "order5_first.ab")), Is.True);
            Assert.That(File.Exists(Path.Combine(_output, "old.ab")), Is.False);

            settings = AssetBundleBuilderSettings.Instance;
            settings.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            settings.buildDirectories = new[] { directoryB };
            Assert.That(AssetBundleBuilderUtility.CreateAssetBundleBuildBySettings()
                .Any(build => build.assetBundleName == "order5_second.ab"), Is.True);
            AssetBundleBuilderUtility.BuildBySettings(_output);
            Assert.That(File.Exists(Path.Combine(_output, "order5_first.ab")), Is.False);
            var bundleFile = Path.Combine(_output, "order5_second.ab");
            Assert.That(File.Exists(bundleFile), Is.True);
            var manifest = ConfigHelper.Load<EasyFramework.AssetBundleManifest>(Path.Combine(_output, EasyFramework.AssetBundleManifest.FileName));
            Assert.That(manifest.abNames.Contains("order5_second.ab"), Is.True);
            var bundle = UnityEngine.AssetBundle.LoadFromFile(bundleFile);
            try { Assert.That(bundle.LoadAsset<UnityEngine.TextAsset>("order5_second").text, Is.EqualTo("second bundle")); }
            finally { if (bundle != null) bundle.Unload(true); }
        }

        [Test]
        public void PipelineReturnsNullManifest_ThrowsAndPreservesToolVersionAndOutput()
        {
            CreateBuildAsset();
            var pipelineType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEditor.Build.Pipeline.ContentPipeline"))
                .Single(type => type != null);
            var callbacks = pipelineType.GetField("BuildCallbacks").GetValue(null);
            var callbackProperty = callbacks.GetType().GetProperty("PostScriptsCallbacks");
            var previous = callbackProperty.GetValue(callbacks);
            var invoke = callbackProperty.PropertyType.GetMethod("Invoke");
            var arguments = invoke.GetParameters().Select(parameter => Expression.Parameter(parameter.ParameterType)).ToArray();
            int calls = 0;
            Action record = () => calls++;
            var body = Expression.Block(Expression.Invoke(Expression.Constant(record)),
                Expression.Constant(Enum.Parse(invoke.ReturnType, "Canceled")));
            callbackProperty.SetValue(callbacks, Expression.Lambda(callbackProperty.PropertyType, body, arguments).Compile());
            var pathField = typeof(AssetBundleBuilder).GetField("ProjectPlatformPath");
            var previousPath = pathField.GetValue(AssetBundleBuilder.Instance);
            var previousIndex = AssetBundleBuilder.Instance.Version.buildIndex;
            pathField.SetValue(AssetBundleBuilder.Instance, _output);
            try
            {
                Assert.Throws<BuildFailedException>(() => AssetBundleBuilder.Instance.Execute());
                Assert.That(calls, Is.EqualTo(1), "The real SBP failure callback must execute.");
                Assert.That(AssetBundleBuilder.Instance.Version.buildIndex, Is.EqualTo(previousIndex));
                AssertPreviousOutput();
            }
            finally
            {
                pathField.SetValue(AssetBundleBuilder.Instance, previousPath);
                callbackProperty.SetValue(callbacks, previous);
            }
        }

        [Test]
        public void ManifestOnly_PreservesBundleOutputAndExistingAssetGuid()
        {
            CreateBuildAsset();
            var pathField = typeof(AssetBundleBuilder).GetField("AssetsPlatformPath");
            var previousPath = pathField.GetValue(AssetBundleBuilder.Instance);
            pathField.SetValue(AssetBundleBuilder.Instance, _assetRoot + "/Manifest");
            try
            {
                AssetBundleBuilderUtility.BuildManifestOnly();
                var manifestPath = AssetBundleBuilder.Instance.AssetsPlatformPath + "/" + PlatformHelper.PlatformName + ".asset";
                var guid = AssetDatabase.AssetPathToGUID(manifestPath);
                Assert.That(guid, Is.Not.Empty);
                AssetBundleBuilderUtility.BuildManifestOnly();
                Assert.That(AssetDatabase.AssetPathToGUID(manifestPath), Is.EqualTo(guid));
                Assert.That(AssetDatabase.LoadMainAssetAtPath(manifestPath), Is.Not.Null);
                AssertPreviousOutput();
            }
            finally { pathField.SetValue(AssetBundleBuilder.Instance, previousPath); }
        }

        private void CreateBuildAsset()
        {
            _assetRoot = "Assets/__EasyFrameworkABSuccess_" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(_assetRoot);
            File.WriteAllText(_assetRoot + "/order5_failure.txt", "real pipeline input");
            AssetDatabase.Refresh();
            AssetBundleBuilderSettings.Instance.buildDirectories = new[] { _assetRoot };
            AssetBundleBuilderSettings.Instance.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
        }

        private void AssertPreviousOutput()
        {
            Assert.That(File.ReadAllText(Path.Combine(_output, "old.ab")), Is.EqualTo("previous usable bundle"));
            Assert.That(File.ReadAllText(Path.Combine(_output, EasyFramework.AssetBundleManifest.FileName)), Is.EqualTo("previous manifest"));
            Assert.That(Directory.GetFiles(_output), Has.Length.EqualTo(2));
        }
    }
}
