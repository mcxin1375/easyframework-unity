using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EasyFramework.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Tests
{
    public class BuildPublicationTests
    {
        private string _root;
        private string _source;
        private string _versions;
        private string _streaming;
        private string _observerAsset;
        private string _oldStreaming;
        private string _settingsJson;
        private ToolVersion _oldVersion;
        private readonly List<Action> _restore = new();

        [SetUp]
        public void SetUp()
        {
            _root = Path.GetFullPath($"Library/Order5Validation/Publication-{Guid.NewGuid():N}");
            _source = Path.Combine(_root, "Source");
            _versions = Path.Combine(_root, "Versions");
            _streaming = Path.Combine(_root, "StreamingAssets");
            Directory.CreateDirectory(_source);
            Directory.CreateDirectory(_versions);
            Directory.CreateDirectory(_streaming);
            File.WriteAllText(Path.Combine(_source, "Cube.ab"), "original bundle");
            File.WriteAllText(Path.Combine(_streaming, "old.txt"), "usable installed content");
            _settingsJson = JsonUtility.ToJson(DLCBuilderSettings.Instance);
            _oldVersion = new ToolVersion();
            _oldVersion.SetToolVersion(DLCBuilder.Instance.Version);
            DLCBuilder.Instance.Version.buildIndex = 501;
            DLCBuilder.Instance.Version.dateTime = DateTime.Now.ToFileTime();
            var settings = DLCBuilderSettings.Instance;
            settings.buildName = EDLCNameOptions.ToolVersion;
            settings.buildMode = EDLCModeOptions.DLC;
            settings.maxCacheNum = 10;
            settings.releaseVersion = null;
            settings.copyVersionToStreamingAssets = false;
            Redirect(AssetBundleBuilder.Instance, "ProjectPlatformPath", _source);
            Redirect(DLCBuilder.Instance, "ProjectPlatformPath", _versions);
            Redirect(DLCBuilder.Instance, "ToolsPlatformPath", Path.Combine(_root, "Tools"));
            var streamingProperty = typeof(EasyFrameworkSettings).GetProperty("StreamingAssetsDLCPath");
            _oldStreaming = (string)streamingProperty.GetValue(EasyFrameworkSettings.Instance);
            streamingProperty.SetValue(EasyFrameworkSettings.Instance, _streaming);
            // When HybridCLR is enabled, use an isolated source rather than the project's DLL outputs.
            var hybridType = typeof(DLCBuilder).Assembly.GetType("EasyFramework.Editor.HybridCLRBuilder");
            if (hybridType != null)
            {
                var hybrid = ReflectionUtility.FindFieldOrProperty<ITool>(hybridType);
                var hybridSource = Path.Combine(_root, "HybridSource");
                Directory.CreateDirectory(hybridSource);
                Redirect(hybrid, "ProjectPlatformPath", hybridSource);
            }
            BuildLifecycleTestExtension.Calls.Clear();
            BuildLifecycleTestExtension.FailurePhase = null;
        }

        [TearDown]
        public void TearDown()
        {
            BuildLifecycleTestExtension.FailurePhase = null;
            BuildLifecycleTestExtension.Calls.Clear();
            if (_observerAsset != null) AssetDatabase.DeleteAsset(_observerAsset);
            ToolExtension<IToolEvent<DLCBuilder>>.Refresh();
            JsonUtility.FromJsonOverwrite(_settingsJson, DLCBuilderSettings.Instance);
            DLCBuilder.Instance.Version.SetToolVersion(_oldVersion);
            typeof(EasyFrameworkSettings).GetProperty("StreamingAssetsDLCPath")
                .SetValue(EasyFrameworkSettings.Instance, _oldStreaming);
            for (int i = _restore.Count - 1; i >= 0; i--) _restore[i]();
            _restore.Clear();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Test]
        public void BuildDLC_MissingSource_PreservesPreviousPackage()
        {
            var output = ExistingPackage();
            Assert.Throws<DirectoryNotFoundException>(() => DLCBuilderUtility.BuildDLC(output,
                new[] { Path.Combine(_root, "Missing") }, out _));
            Assert.That(File.ReadAllText(Path.Combine(output, "old.ab")), Is.EqualTo("previous package"));
            Assert.That(Directory.GetFiles(output), Has.Length.EqualTo(1));
        }

        [Test]
        public void BuildDLC_UnreadableSource_PreservesPreviousPackage()
        {
            var output = ExistingPackage();
            using (File.Open(Path.Combine(_source, "Cube.ab"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Assert.Throws<IOException>(() => DLCBuilderUtility.BuildDLC(output, new[] { _source }, out _));
            Assert.That(File.ReadAllText(Path.Combine(output, "old.ab")), Is.EqualTo("previous package"));
            Assert.That(Directory.GetFiles(output), Has.Length.EqualTo(1));
        }

        [Test]
        public void BuildDLC_ConflictingResourceNames_RejectsAmbiguousManifest()
        {
            var other = Path.Combine(_root, "OtherSource");
            Directory.CreateDirectory(other);
            File.WriteAllText(Path.Combine(other, "Cube.ab"), "different bytes");
            var output = ExistingPackage();
            Assert.Throws<InvalidDataException>(() => DLCBuilderUtility.BuildDLC(output, new[] { _source, other }, out _));
            Assert.That(File.ReadAllText(Path.Combine(output, "old.ab")), Is.EqualTo("previous package"));
        }

        [Test]
        public void BuildDLC_Success_PublishesOnlyVerifiedFiles()
        {
            var output = ExistingPackage();
            DLCBuilderUtility.BuildDLC(output, new[] { _source }, out var uid);
            var info = ConfigHelper.Load<DLCVersionInfo>(Path.Combine(output, DLCVersionInfo.FileName));
            Assert.That(info.uid, Is.EqualTo(uid));
            Assert.That(info.hashFiles, Has.Length.EqualTo(1));
            Assert.That(File.Exists(Path.Combine(output, "old.ab")), Is.False);
            AssertValidFiles(output, info);
        }

        [Test]
        public void BuildBySettings_Success_PublishesMatchingVersionAndStreamingAssets()
        {
            DLCBuilderSettings.Instance.copyVersionToStreamingAssets = true;
            DLCBuilderUtility.BuildBySettings(_versions);
            var latest = ConfigHelper.Load<DLCVersion>(Path.Combine(_versions, DLCVersion.LatestFileName));
            var installed = ConfigHelper.Load<DLCVersion>(Path.Combine(_streaming, DLCVersion.FileName));
            Assert.That(latest.versionName, Is.EqualTo("501"));
            Assert.That(installed.versionUid, Is.EqualTo(latest.versionUid));
            Assert.That(File.Exists(Path.Combine(_streaming, "old.txt")), Is.False);
            var info = ConfigHelper.Load<DLCVersionInfo>(Path.Combine(_streaming, DLCVersionInfo.FileName));
            Assert.That(info.uid, Is.EqualTo(latest.dlcVersionInfoUid));
            AssertValidFiles(_streaming, info);
            Assert.That(Directory.GetFileSystemEntries(_versions).Any(path => Path.GetFileName(path).StartsWith(".")), Is.False);
        }

        [TestCase(DLCVersion.LatestFileName)]
        [TestCase(DLCBuilderVersionList.FileName)]
        public void BuildBySettings_CommitFailure_RestoresLatestHistoryAndStreaming(string lockedMetadata)
        {
            DLCBuilderSettings.Instance.copyVersionToStreamingAssets = true;
            DLCBuilderSettings.Instance.maxCacheNum = 1;
            DLCBuilderUtility.BuildBySettings(_versions);
            var original = Snapshot(_root);
            DLCBuilder.Instance.Version.buildIndex = 502;
            File.WriteAllText(Path.Combine(_source, "Cube.ab"), "changed bundle");
            using (File.Open(Path.Combine(_versions, lockedMetadata), FileMode.Open, FileAccess.Read, FileShare.Read))
                Assert.Throws<IOException>(() => DLCBuilderUtility.BuildBySettings(_versions));
            var after = Snapshot(_root);
            original.Remove("Source/Cube.ab");
            after.Remove("Source/Cube.ab");
            Assert.That(after, Is.EquivalentTo(original), "A failed metadata swap must restore version directories and all publication pointers.");
        }

        [Test]
        public void BuildBySettings_InvalidRelease_DoesNotPublishOrTouchInstalledContent()
        {
            DLCBuilderSettings.Instance.copyVersionToStreamingAssets = true;
            DLCBuilderSettings.Instance.releaseVersion = "does-not-exist";
            var before = Snapshot(_root);
            Assert.Throws<InvalidDataException>(() => DLCBuilderUtility.BuildBySettings(_versions));
            Assert.That(Snapshot(_root), Is.EquivalentTo(before));
        }

        [Test]
        public void BuildBySettings_CachePruning_KeepsSelectedRelease()
        {
            DLCBuilderUtility.BuildBySettings(_versions);
            DLCBuilder.Instance.Version.buildIndex = 502;
            DLCBuilderSettings.Instance.maxCacheNum = 1;
            DLCBuilderSettings.Instance.releaseVersion = "501";
            DLCBuilderSettings.Instance.copyVersionToStreamingAssets = true;
            DLCBuilderUtility.BuildBySettings(_versions);
            Assert.That(Directory.Exists(Path.Combine(_versions, "501")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_versions, "502")), Is.True);
            var installed = ConfigHelper.Load<DLCVersion>(Path.Combine(_streaming, DLCVersion.FileName));
            Assert.That(installed.versionName, Is.EqualTo("501"));
            DLCBuilderSettings.Instance.releaseVersion = null;
            DLCBuilder.Instance.Version.buildIndex = 503;
            DLCBuilderUtility.BuildBySettings(_versions);
            Assert.That(Directory.Exists(Path.Combine(_versions, "501")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(_versions, "502")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(_versions, "503")), Is.True);
        }

        [TestCase("missing")]
        [TestCase("../Source")]
        public void CopyVersion_InvalidSelection_PreservesStreamingAssets(string selection)
        {
            var before = Snapshot(_streaming);
            Assert.Throws<InvalidDataException>(() => DLCBuilderUtility.CopyVersionToStreamingAssets(selection));
            Assert.That(Snapshot(_streaming), Is.EquivalentTo(before));
        }

        [TestCase("missing-file")]
        [TestCase("wrong-hash")]
        [TestCase("wrong-uid")]
        [TestCase("unreadable-file")]
        public void CopyVersion_InvalidOrUnreadableContent_PreservesStreamingAssets(string failure)
        {
            DLCBuilderUtility.BuildBySettings(_versions);
            var package = Path.Combine(_versions, "501", "DLC");
            var manifestFile = Path.Combine(package, DLCVersionInfo.FileName);
            var info = ConfigHelper.Load<DLCVersionInfo>(manifestFile);
            var resource = Path.Combine(package, info.hashFiles[0].fileName);
            if (failure == "missing-file") File.Delete(resource);
            if (failure == "wrong-hash") File.WriteAllText(resource, "corrupt contents");
            if (failure == "wrong-uid")
            {
                info.uid = "wrong uid";
                ConfigHelper.Save(info, manifestFile);
            }
            var before = Snapshot(_streaming);
            using (failure == "unreadable-file" ? File.Open(resource, FileMode.Open, FileAccess.ReadWrite, FileShare.None) : null)
            {
                if (failure == "unreadable-file") Assert.Throws<IOException>(() => DLCBuilderUtility.CopyVersionToStreamingAssets("501"));
                else Assert.Throws<InvalidDataException>(() => DLCBuilderUtility.CopyVersionToStreamingAssets("501"));
            }
            Assert.That(Snapshot(_streaming), Is.EquivalentTo(before));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DLCExecute_DirectOrParentInterface_RunsLifecycleExactlyOnce(bool throughParentInterface)
        {
            ObserveLifecycle();
            if (throughParentInterface) ((IToolEvent<AssetBuilder>)DLCBuilder.Instance).OnExecute();
            else DLCBuilder.Instance.Execute();
            Assert.That(BuildLifecycleTestExtension.Calls, Is.EqualTo(new[] { "DLC.Before", "DLC.Execute", "DLC.After" }));
        }

        [Test]
        public void UtilityBuild_DoesNotDispatchToolLifecycle()
        {
            ObserveLifecycle();
            DLCBuilderUtility.BuildBySettings(_versions);
            Assert.That(BuildLifecycleTestExtension.Calls, Is.Empty);
        }

        [Test]
        public void DLCExecute_BeforePublicationFailure_PreservesOutputsAndToolVersion()
        {
            DLCBuilderSettings.Instance.copyVersionToStreamingAssets = true;
            DLCBuilderUtility.BuildBySettings(_versions);
            var before = Snapshot(_root);
            var previousIndex = DLCBuilder.Instance.Version.buildIndex;
            ObserveLifecycle();
            BuildLifecycleTestExtension.FailurePhase = "DLC.Execute";
            Assert.Throws<InvalidOperationException>(() => DLCBuilder.Instance.Execute());
            Assert.That(DLCBuilder.Instance.Version.buildIndex, Is.EqualTo(previousIndex));
            Assert.That(Snapshot(_root), Is.EquivalentTo(before));
            BuildLifecycleTestExtension.FailurePhase = null;
            DLCBuilder.Instance.Execute();
            Assert.That(DLCBuilder.Instance.Version.buildIndex, Is.EqualTo(previousIndex + 1));
        }

        [Test]
        public void DLCExecute_AfterCallbackFailure_StopsToolWithoutUndoingCompletedPublication()
        {
            DLCBuilderSettings.Instance.copyVersionToStreamingAssets = true;
            DLCBuilderUtility.BuildBySettings(_versions);
            var previousIndex = DLCBuilder.Instance.Version.buildIndex;
            ObserveLifecycle();
            BuildLifecycleTestExtension.FailurePhase = "DLC.After";
            Assert.Throws<InvalidOperationException>(() => DLCBuilder.Instance.Execute());
            Assert.That(DLCBuilder.Instance.Version.buildIndex, Is.EqualTo(previousIndex));
            var latest = ConfigHelper.Load<DLCVersion>(Path.Combine(_versions, DLCVersion.LatestFileName));
            var installed = ConfigHelper.Load<DLCVersion>(Path.Combine(_streaming, DLCVersion.FileName));
            Assert.That(latest.versionName, Is.EqualTo((previousIndex + 1).ToString()));
            Assert.That(installed.versionUid, Is.EqualTo(latest.versionUid));
            Assert.That(File.Exists(Path.Combine(_root, "Tools", ToolVersion.FileName)), Is.False);
        }

        private void ObserveLifecycle()
        {
            _observerAsset = $"Assets/__EasyFrameworkDLCEvents_{Guid.NewGuid():N}.asset";
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BuildLifecycleTestExtension>(), _observerAsset);
            ToolExtension<IToolEvent<DLCBuilder>>.Refresh();
        }

        private string ExistingPackage()
        {
            var output = Path.Combine(_root, "Package");
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "old.ab"), "previous package");
            return output;
        }

        private void Redirect(object tool, string name, string value)
        {
            var field = tool.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            var previous = field.GetValue(tool);
            field.SetValue(tool, value);
            _restore.Add(() => field.SetValue(tool, previous));
        }

        private static void AssertValidFiles(string directory, DLCVersionInfo info)
        {
            foreach (var file in info.hashFiles)
                Assert.That(FileHelper.VerifyFileIntegrity(Path.Combine(directory, file.fileName), file.length,
                    FileHelper.ExtractMD5FromFileName(file.fileName)), Is.True, file.resName);
        }

        private static Dictionary<string, string> Snapshot(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(file => file.Substring(root.Length + 1).Replace('\\', '/'), MD5Helper.MD5File);
    }
}
