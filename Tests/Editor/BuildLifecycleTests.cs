using System;
using System.IO;
using EasyFramework.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Tests
{
    public class BuildLifecycleTests
    {
        private string _assetPath;
        private ToolVersion _assetVersion;
        private string _assetVersionFile;
        private byte[] _assetVersionBytes;
        private bool _buildAssets;

        [SetUp]
        public void SetUp()
        {
            BuildLifecycleTestExtension.Calls.Clear();
            BuildLifecycleTestExtension.FailurePhase = null;
            _assetVersion = new ToolVersion();
            _assetVersion.SetToolVersion(AssetBuilder.Instance.Version);
            _assetVersionFile = Path.Combine(AssetBuilder.Instance.ToolsPlatformPath, ToolVersion.FileName);
            _assetVersionBytes = File.Exists(_assetVersionFile) ? File.ReadAllBytes(_assetVersionFile) : null;
            _buildAssets = PlayerBuilderSettings.Instance.assetBuilder;
            _assetPath = "Assets/__EasyFrameworkBuildLifecycle_" + Guid.NewGuid().ToString("N") + ".asset";
            var extension = ScriptableObject.CreateInstance<BuildLifecycleTestExtension>();
            AssetDatabase.CreateAsset(extension, _assetPath);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerBuilderSettings.Instance.assetBuilder = _buildAssets;
            BuildLifecycleTestExtension.FailurePhase = null;
            BuildLifecycleTestExtension.Calls.Clear();
            AssetDatabase.DeleteAsset(_assetPath);
            AssetBuilder.Instance.Version.SetToolVersion(_assetVersion);
            if (_assetVersionBytes == null)
            {
                if (File.Exists(_assetVersionFile)) File.Delete(_assetVersionFile);
            }
            else
            {
                File.WriteAllBytes(_assetVersionFile, _assetVersionBytes);
            }
            ToolExtension<IToolEvent<AssetBuilder>>.Refresh();
            ToolExtension<IToolEvent<PlayerBuilder>>.Refresh();
        }

        [TestCase("Asset.Before", new[] { "Asset.Before" })]
        [TestCase("Asset.Execute", new[] { "Asset.Before", "Asset.Execute" })]
        public void AssetStageFailure_PropagatesAndSkipsSubsequentStages(string failurePhase, string[] expectedCalls)
        {
            var bundleVersion = AssetBundleBuilder.Instance.Version.buildIndex;
            var dlcVersion = DLCBuilder.Instance.Version.buildIndex;
            BuildLifecycleTestExtension.FailurePhase = failurePhase;

            var exception = Assert.Throws<InvalidOperationException>(() => AssetBuilder.Instance.Execute());

            Assert.That(exception.Message, Is.EqualTo("Injected build failure: " + failurePhase));
            Assert.That(BuildLifecycleTestExtension.Calls, Is.EqualTo(expectedCalls));
            Assert.That(AssetBundleBuilder.Instance.Version.buildIndex, Is.EqualTo(bundleVersion));
            Assert.That(DLCBuilder.Instance.Version.buildIndex, Is.EqualTo(dlcVersion));
        }

        [TestCase("Asset.Before")]
        [TestCase("Asset.Execute")]
        public void AssetStageFailure_DuringPlayerPrebuild_PreventsPlayerBuild(string failurePhase)
        {
            PlayerBuilderSettings.Instance.assetBuilder = true;
            BuildLifecycleTestExtension.FailurePhase = failurePhase;
            var playerVersion = PlayerBuilder.Instance.Version.buildIndex;
            var dlcVersion = DLCBuilder.Instance.Version.buildIndex;

            Assert.Throws<InvalidOperationException>(() => PlayerBuilder.Instance.Execute());

            Assert.That(BuildLifecycleTestExtension.Calls[0], Is.EqualTo("Player.Before"));
            Assert.That(BuildLifecycleTestExtension.Calls, Does.Not.Contain("Player.Execute"));
            Assert.That(BuildLifecycleTestExtension.Calls, Does.Not.Contain("Player.After"));
            Assert.That(BuildLifecycleTestExtension.Calls, Does.Not.Contain("Asset.After"));
            Assert.That(PlayerBuilder.Instance.Version.buildIndex, Is.EqualTo(playerVersion));
            Assert.That(DLCBuilder.Instance.Version.buildIndex, Is.EqualTo(dlcVersion));
        }
    }
}
