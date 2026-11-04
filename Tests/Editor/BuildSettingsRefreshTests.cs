using System;
using System.IO;
using System.Linq;
using EasyFramework.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Tests
{
    public class BuildSettingsRefreshTests
    {
        private string _root;
        private string _directoryA;
        private string _directoryB;
        private string _directoryC;
        private string _assetA;
        private string _assetB;
        private string _assetC;
        private string[] _previousDirectories;

        [SetUp]
        public void SetUp()
        {
            _previousDirectories = AssetBundleBuilderSettings.Instance.buildDirectories;
            AssetBundleBuilderSettings.Instance.buildDirectories = Array.Empty<string>();
            _root = $"Assets/__EasyFrameworkBuildSettings_{Guid.NewGuid():N}";
            _directoryA = $"{_root}/A";
            _directoryB = $"{_root}/B";
            _directoryC = $"{_root}/C";
            Directory.CreateDirectory(_directoryA);
            Directory.CreateDirectory(_directoryB);
            Directory.CreateDirectory(_directoryC);
            _assetA = CreateTextAsset(_directoryA, "settings_a");
            _assetB = CreateTextAsset(_directoryB, "settings_b");
            _assetC = CreateTextAsset(_directoryC, "settings_c");
            AssetDatabase.Refresh();
            ToolExtension<IAssetBundleBuilderSettings>.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetBundleBuilderSettings.Instance.buildDirectories = _previousDirectories;
            if (!string.IsNullOrEmpty(_root)) AssetDatabase.DeleteAsset(_root);
            ToolExtension<IAssetBundleBuilderSettings>.Refresh();
        }

        [Test]
        public void CachedDefaultSettings_ReadsCurrentDirectories()
        {
            var settings = ToolExtension<IAssetBundleBuilderSettings>.Instances
                .Single(item => item.GetType().Name == "AssetBundleBuildSettingExtension");
            AssetBundleBuilderSettings.Instance.buildDirectories = new[] { _directoryA };
            CollectionAssert.AreEqual(new[] { _directoryA }, settings.BuildDirectories);

            AssetBundleBuilderSettings.Instance.buildDirectories = new[] { _directoryB };
            CollectionAssert.AreEqual(new[] { _directoryB }, settings.BuildDirectories);

            AssetBundleBuilderSettings.Instance.buildDirectories = null;
            Assert.That(settings.BuildDirectories, Is.Null);
        }

        [Test]
        public void BuildCollection_UsesChangedDefaultDirectoriesInSameSession()
        {
            AssetBundleBuilderSettings.Instance.buildDirectories = new[] { _directoryA };
            AssertContains(_assetA, true);
            AssertContains(_assetB, false);

            AssetBundleBuilderSettings.Instance.buildDirectories = new[] { _directoryB };
            AssertContains(_assetA, false);
            AssertContains(_assetB, true);
        }

        [Test]
        public void BuildCollection_DiscoversSettingsCreatedAfterFirstCollection()
        {
            AssertContains(_assetC, false);
            CreateSettings(_directoryC);
            AssertContains(_assetC, true);
        }

        [Test]
        public void BuildCollection_ReadsChangedScriptableSettingsInSameSession()
        {
            var settings = CreateSettings(_directoryA);
            AssertContains(_assetA, true);

            settings.buildDirectories = new[] { _directoryB };
            AssertContains(_assetA, false);
            AssertContains(_assetB, true);
        }

        [Test]
        public void BuildCollection_ExcludesDisabledDirectoriesAndBuildInfos()
        {
            var settings = CreateSettings(_directoryC);
            settings.buildInfos = new[]
            {
                new AssetBundleBuildInfo
                {
                    abName = "disabled-settings-info",
                    directories = new[] { _directoryB }
                }
            };
            AssertContains(_assetC, true);
            AssertContains(_assetB, true);

            settings.enabled = false;
            AssertContains(_assetC, false);
            AssertContains(_assetB, false);
        }

        [Test]
        public void BuildCollection_DropsDeletedSettingsInSameSession()
        {
            var settings = CreateSettings(_directoryC);
            AssertContains(_assetC, true);

            Assert.That(AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(settings)), Is.True);
            AssertContains(_assetC, false);
        }

        private string CreateTextAsset(string directory, string name)
        {
            var path = $"{directory}/{name}.txt";
            File.WriteAllText(path, name);
            return path;
        }

        private AssetBundleBuildSettings CreateSettings(string directory)
        {
            var settings = ScriptableObject.CreateInstance<AssetBundleBuildSettings>();
            settings.buildDirectories = new[] { directory };
            AssetDatabase.CreateAsset(settings, $"{_root}/Settings.asset");
            return settings;
        }

        private void AssertContains(string asset, bool expected)
        {
            var builds = AssetBundleBuilderUtility.CreateAssetBundleBuildBySettings();
            var included = builds.Any(build => build.assetNames.Any(name => name.Replace('\\', '/') == asset));
            Assert.That(included, Is.EqualTo(expected), asset);
        }
    }
}
