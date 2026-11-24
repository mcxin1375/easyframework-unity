using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EasyFramework.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.TestTools;

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
        public void NameToKey_ReturnsLowercaseNameWithConfiguredSuffix()
        {
            var settings = EasyFrameworkSettings.Instance;
            var previousSuffix = settings.abSuffix;
            try
            {
                settings.abSuffix = ".AB";
                Assert.That(AssetBundleLoaderHelper.NameToKey("Res_AA"), Is.EqualTo("res_aa.ab"));
                Assert.That(AssetBundleLoaderHelper.NameToKey("Res_aa"), Is.EqualTo("res_aa.ab"));
                Assert.That(AssetBundleLoaderHelper.NameToKey("Res_AA.Ab"), Is.EqualTo("res_aa.ab"));
                Assert.That(AssetBundleLoaderHelper.NameToKey("res_aa.ab"), Is.EqualTo("res_aa.ab"));

                settings.abSuffix = ".Bundle.AB";
                Assert.That(AssetBundleLoaderHelper.NameToKey("Res_AA"), Is.EqualTo("res_aa.bundle.ab"));
                Assert.That(AssetBundleLoaderHelper.NameToKey("Res_aa.bundle.AB"), Is.EqualTo("res_aa.bundle.ab"));
            }
            finally
            {
                settings.abSuffix = previousSuffix;
            }
        }

        [Test]
        public void CaseVariants_CacheOriginalKeysAndShareLegacyBundleInfo()
        {
            var loader = new AssetBundleLoader();
            try
            {
                var dependency = new AssetBundleInfo("Shared.AB", null, null);
                var info = new AssetBundleInfo("Res_AA.AB", new[] { dependency.FileName }, null);
                var bundles = (IDictionary<string, AssetBundleInfo>)loader.AbDict;
                bundles.Add(info.FileName, info);
                bundles.Add(dependency.FileName, dependency);

                Assert.That(loader.GetAllDependencies("Res_AA"), Is.SameAs(info.DepNames));
                var dependencies = info.Dependencies;
                Assert.That(dependencies, Is.EqualTo(new[] { dependency }));
                Assert.That(loader.GetAllDependencies("Res_aa"), Is.SameAs(info.DepNames));
                Assert.That(info.Dependencies, Is.SameAs(dependencies));

                var keys = (IReadOnlyDictionary<string, AssetBundleInfo>)typeof(AssetBundleLoader)
                    .GetField("_keyDict", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(loader);
                Assert.That(keys.Keys, Is.EquivalentTo(new[] { "Res_AA", "Res_aa" }));
                Assert.That(keys["Res_AA"], Is.SameAs(info));
                Assert.That(keys["Res_aa"], Is.SameAs(info));
            }
            finally
            {
                ETask.RemoveTick(loader);
            }
        }

        [UnityTest]
        public IEnumerator CaseVariantsAndRepeatedNames_ShareOneLowercaseBundleAndLoadByOriginalCase()
        {
            _assetRoot = "Assets/__EasyFrameworkABCase_" + Guid.NewGuid().ToString("N");
            var assets = new[]
            {
                (Path: "A/Res_AA.txt", Text: "upper A"),
                (Path: "B/Res_aa.txt", Text: "lower"),
                (Path: "C/Res_AA.txt", Text: "upper C"),
                (Path: "D/Res_AA.txt", Text: "upper D")
            };
            foreach (var asset in assets)
            {
                var path = _assetRoot + "/" + asset.Path;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, asset.Text);
            }
            AssetDatabase.Refresh();
            var settings = AssetBundleBuilderSettings.Instance;
            settings.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            settings.buildDirectories = new[] { _assetRoot };

            const string bundleName = "res_aa.ab";
            var build = AssetBundleBuilderUtility.CreateAssetBundleBuildsByDirectories(_assetRoot).Single();
            Assert.That(build.assetBundleName, Is.EqualTo(bundleName));
            Assert.That(build.addressableNames, Is.EquivalentTo(new[] { "Res_AA", "Res_aa", "Res_AA", "Res_AA" }));
            var uppercaseTexts = new[] { "upper A", "upper C", "upper D" };

            var editorLoaderType = typeof(AssetBundleBuilderUtility).Assembly.GetType(
                "EasyFramework.Editor.AssetBundleLoaderEditor", true);
            var editorLoader = (IResLoader)Activator.CreateInstance(editorLoaderType);
            Assert.That(uppercaseTexts, Does.Contain(editorLoader.LoadAsset<TextAsset>("Res_AA")?.text));
            Assert.That(editorLoader.LoadAsset<TextAsset>("Res_aa")?.text, Is.EqualTo("lower"));
            Assert.That(editorLoader.LoadAsset<TextAsset>("res_aa", "Res_aa")?.text, Is.EqualTo("lower"));
            Assert.That(editorLoader.LoadAsset<TextAsset>("RES_AA"), Is.Null);

            AssetBundleBuilderUtility.BuildBySettings(_output);
            var manifest = ConfigHelper.Load<EasyFramework.AssetBundleManifest>(
                Path.Combine(_output, EasyFramework.AssetBundleManifest.FileName));
            Assert.That(manifest.abNames.Where(name => string.Equals(name, bundleName, StringComparison.OrdinalIgnoreCase)),
                Is.EqualTo(new[] { bundleName }));
            Assert.That(Directory.EnumerateFiles(_output).Select(Path.GetFileName), Does.Contain(bundleName));

            var loader = new AssetBundleLoader();
            var runtimeSettings = EasyFrameworkSettings.Instance;
            var previousMode = runtimeSettings.resLoaderMode;
            try
            {
                runtimeSettings.resLoaderMode = EResLoaderMode.DLC_StreamingAssets;
                ((IDictionary<string, AssetBundleInfo>)loader.AbDict).Add(bundleName,
                    new AssetBundleInfo(bundleName, manifest.GetAllDependencies(bundleName), Path.Combine(_output, bundleName)));

                // Drive ETask manually in EditMode.
                var tick = (Action)typeof(ETask).GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Static)
                    .CreateDelegate(typeof(Action));
                var lowercase = loader.LoadAssetAsync<TextAsset>("Res_aa").GetAwaiter();
                var deadline = EditorApplication.timeSinceStartup + 10;
                while (!lowercase.IsCompleted && EditorApplication.timeSinceStartup < deadline)
                {
                    tick();
                    yield return null;
                }
                Assert.That(lowercase.IsCompleted, Is.True);
                Assert.That(lowercase.GetResult()?.text, Is.EqualTo("lower"));
                Assert.That(uppercaseTexts, Does.Contain(loader.LoadAsset<TextAsset>("Res_AA")?.text));
                Assert.That(loader.LoadAsset<TextAsset>("res_aa", "Res_aa")?.text, Is.EqualTo("lower"));
                Assert.That(loader.LoadAsset<TextAsset>("RES_AA"), Is.Null);
                Assert.That(loader.Load("Res_AA"), Is.SameAs(loader.Load("Res_aa")));
                Assert.That(loader.LoadAllAssets<TextAsset>(bundleName).Select(asset => asset.text),
                    Is.EquivalentTo(assets.Select(asset => asset.Text)));
            }
            finally
            {
                loader.UnloadAllForce(true);
                ETask.RemoveTick(loader);
                runtimeSettings.resLoaderMode = previousMode;
            }
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
