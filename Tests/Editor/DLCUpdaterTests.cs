using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EasyFramework.Tests
{
    public class DLCUpdaterTests
    {
        private Sandbox _sandbox;
        private IDebugger _originalDebugger;

        [SetUp]
        public void SetUp()
        {
            _originalDebugger = FDebug.Debugger;
            FDebug.Debugger = new TestDebugger();
            _sandbox = new Sandbox();
        }

        [TearDown]
        public void TearDown()
        {
            try { _sandbox.Dispose(); }
            finally { FDebug.Debugger = _originalDebugger; }
        }

        [Test]
        public void Exists_BeforeInitializationOrForUnknownResource_IsFalse()
        {
            Assert.That(_sandbox.Updater.Exists("unknown.ab"), Is.False);
        }

        [Test]
        public void Exists_ValidatesLengthAndContentHash() => Complete(Exists_ValidatesLengthAndContentHashAsync());

        private async Task Exists_ValidatesLengthAndContentHashAsync()
        {
            var info = _sandbox.File("Cube.ab", "good");
            await _sandbox.Initialize(info);
            var path = _sandbox.PathFor(info);
            Assert.That(_sandbox.Updater.Exists(info.resName), Is.False);
            System.IO.File.WriteAllText(path, "bad");
            Assert.That(_sandbox.Updater.Exists(info.resName), Is.False, "A truncated cache must not count as available.");
            System.IO.File.WriteAllText(path, "evil");
            Assert.That(_sandbox.Updater.Exists(info.resName), Is.False, "Matching length alone must not validate corrupted cache data.");
            System.IO.File.WriteAllText(path, "good");
            Assert.That(_sandbox.Updater.Exists(info.resName), Is.True);
            Assert.That(_sandbox.Updater.Exists("unknown.ab"), Is.False);
        }

        [Test]
        public void Cleanup_OnlyRemovesUnversionedHashFilesAtDLCRoot() => Complete(Cleanup_OnlyRemovesUnversionedHashFilesAtDLCRootAsync());

        private async Task Cleanup_OnlyRemovesUnversionedHashFilesAtDLCRootAsync()
        {
            var keep = _sandbox.File("Cube.ab", "current");
            var stale = _sandbox.File("Old.ab", "stale");
            System.IO.File.WriteAllText(_sandbox.PathFor(keep), "current");
            System.IO.File.WriteAllText(_sandbox.PathFor(stale), "stale");
            var protectedPaths = new[]
            {
                Path.Combine(_sandbox.Root, "business.json"),
                Path.Combine(_sandbox.Root, "notes.txt"),
                Path.Combine(_sandbox.Root, "DLCVersion.json"),
                Path.Combine(_sandbox.Root, "LatestVersion.json"),
                Path.Combine(_sandbox.Root, "AppVersion.json"),
                Path.Combine(_sandbox.Root, "child", stale.fileName),
                Path.Combine(_sandbox.Parent, stale.fileName)
            };
            foreach (var path in protectedPaths)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                System.IO.File.WriteAllText(path, "preserve");
            }

            await _sandbox.Initialize(keep);
            _sandbox.Updater.DeleteUnversionedFiles();

            Assert.That(System.IO.File.Exists(_sandbox.PathFor(stale)), Is.False);
            Assert.That(System.IO.File.Exists(_sandbox.PathFor(keep)), Is.True);
            Assert.That(System.IO.File.Exists(_sandbox.Updater.DLCVersionInfoLocalFile), Is.True);
            foreach (var path in protectedPaths) Assert.That(System.IO.File.ReadAllText(path), Is.EqualTo("preserve"), path);
        }

        [Test]
        public void Cleanup_WithoutValidManifest_DoesNotDeleteAnything()
        {
            var stale = _sandbox.File("Old.ab", "stale");
            System.IO.File.WriteAllText(_sandbox.PathFor(stale), "stale");
            _sandbox.Updater.DeleteUnversionedFiles();
            Assert.That(System.IO.File.ReadAllText(_sandbox.PathFor(stale)), Is.EqualTo("stale"));
        }

        [TestCase("empty-response")]
        [TestCase("null-manifest")]
        [TestCase("missing-array")]
        [TestCase("wrong-uid")]
        [TestCase("parent-path")]
        [TestCase("absolute-path")]
        [TestCase("non-hash")]
        [TestCase("negative-length")]
        [TestCase("duplicate-resource")]
        [TestCase("conflicting-file-length")]
        [TestCase("null-entry")]
        public void Initialize_InvalidManifest_RejectsItWithoutCleanup(string invalidKind) => Complete(Initialize_InvalidManifest_RejectsItWithoutCleanupAsync(invalidKind));

        private async Task Initialize_InvalidManifest_RejectsItWithoutCleanupAsync(string invalidKind)
        {
            var stale = _sandbox.File("Old.ab", "stale");
            System.IO.File.WriteAllText(_sandbox.PathFor(stale), "stale");
            var candidate = _sandbox.Manifest(_sandbox.File("Cube.ab", "good"));
            switch (invalidKind)
            {
                case "missing-array": candidate.hashFiles = null; break;
                case "wrong-uid": candidate.uid = "other"; break;
                case "parent-path": candidate.hashFiles[0].fileName = "../" + stale.fileName; break;
                case "absolute-path": candidate.hashFiles[0].fileName = Path.Combine(_sandbox.Parent, stale.fileName); break;
                case "non-hash": candidate.hashFiles[0].fileName = "business.json"; break;
                case "negative-length": candidate.hashFiles[0].length = -1; break;
                case "duplicate-resource": candidate.hashFiles = new[] { candidate.hashFiles[0], candidate.hashFiles[0] }; break;
                case "conflicting-file-length":
                    candidate.hashFiles = new[] { candidate.hashFiles[0], new HashFileInfo
                    { resName = "Other.ab", fileName = candidate.hashFiles[0].fileName, length = candidate.hashFiles[0].length + 1 } };
                    break;
                case "null-entry": candidate.hashFiles = new HashFileInfo[] { null }; break;
            }
            _sandbox.ManifestText = invalidKind == "empty-response" ? string.Empty
                : invalidKind == "null-manifest" ? "null"
                : invalidKind == "missing-array" ? "{\"uid\":\"v1\"}"
                : ConfigHelper.ToJson(candidate);

            Assert.That(await _sandbox.Updater.InitializeAsync(), Is.False);
            _sandbox.Updater.DeleteUnversionedFiles();

            Assert.That(_sandbox.Updater.VersionInfo, Is.Null);
            Assert.That(System.IO.File.ReadAllText(_sandbox.PathFor(stale)), Is.EqualTo("stale"));
            Assert.That(System.IO.File.Exists(_sandbox.Updater.DLCVersionInfoLocalFile), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Initialize_EmptyVersion_RequiresExplicitFilesArray(bool includeArray) => Complete(Initialize_EmptyVersion_RequiresExplicitFilesArrayAsync(includeArray));

        private async Task Initialize_EmptyVersion_RequiresExplicitFilesArrayAsync(bool includeArray)
        {
            var stale = _sandbox.File("Old.ab", "stale");
            System.IO.File.WriteAllText(_sandbox.PathFor(stale), "stale");
            _sandbox.ManifestText = includeArray ? "{\"uid\":\"v1\",\"hashFiles\":[]}" : "{\"uid\":\"v1\"}";

            Assert.That(await _sandbox.Updater.InitializeAsync(), Is.EqualTo(includeArray));
            Assert.That(System.IO.File.Exists(_sandbox.Updater.DLCVersionInfoLocalFile), Is.EqualTo(includeArray));
            Assert.That(System.IO.File.Exists(_sandbox.PathFor(stale)), Is.EqualTo(!includeArray),
                "Only an explicit valid empty manifest may clean stale DLC resources.");
            if (includeArray) Assert.That(_sandbox.Updater.VersionInfo.hashFiles, Is.Empty);
            else Assert.That(_sandbox.Updater.VersionInfo, Is.Null);
        }

        [Test]
        public void Reinitialize_NewVersion_ResolvesCurrentHashInsteadOfCachedPath() => Complete(Reinitialize_NewVersion_ResolvesCurrentHashInsteadOfCachedPathAsync());

        private async Task Reinitialize_NewVersion_ResolvesCurrentHashInsteadOfCachedPathAsync()
        {
            var v1 = _sandbox.File("Cube.ab", "version-one");
            await _sandbox.Initialize(v1);
            var oldPath = _sandbox.Updater.GetFilePath(v1.resName);
            _sandbox.Uid = "v2";
            var v2 = _sandbox.File("Cube.ab", "version-two");
            await _sandbox.Initialize(v2);
            Assert.That(_sandbox.Updater.GetFilePath(v2.resName), Is.EqualTo(_sandbox.PathFor(v2)));
            Assert.That(_sandbox.Updater.GetFilePath(v2.resName), Is.Not.EqualTo(oldPath));
        }

        [TestCase(null)]
        [TestCase("not-json")]
        [TestCase("{\"uid\":\"v2\",\"hashFiles\":null}")]
        [TestCase("{\"uid\":\"wrong\",\"hashFiles\":[]}")]
        public void Update_NewManifestFailure_PreservesOldMemoryAndDiskAndReportsVersionError(string response) => Complete(Update_NewManifestFailure_PreservesOldMemoryAndDiskAndReportsVersionErrorAsync(response));

        private async Task Update_NewManifestFailure_PreservesOldMemoryAndDiskAndReportsVersionErrorAsync(string response)
        {
            await _sandbox.Initialize(_sandbox.File("Cube.ab", "v1"));
            var originalInfo = _sandbox.Updater.VersionInfo;
            var originalDisk = System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile);
            _sandbox.Uid = "v2";
            _sandbox.ManifestText = response;

            Assert.That(await _sandbox.Updater.UpdateAsync(), Is.EqualTo(DLCUpdater.EResult.VersionError));
            Assert.That(_sandbox.Updater.VersionInfo, Is.SameAs(originalInfo));
            Assert.That(System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile), Is.EqualTo(originalDisk));
            Assert.That(_sandbox.DownloadCount, Is.Zero);
        }

        [Test]
        public void Download_UnknownResource_DoesNotIssueNetworkRequest() => Complete(Download_UnknownResource_DoesNotIssueNetworkRequestAsync());

        private async Task Download_UnknownResource_DoesNotIssueNetworkRequestAsync()
        {
            await _sandbox.Initialize(_sandbox.File("Cube.ab", "good"));
            Assert.That(await _sandbox.Updater.DownloadAsync("unknown.ab"), Is.False);
            Assert.That(await _sandbox.Updater.DownloadAndReturnFileAsync("unknown.ab"), Is.Empty);
            Assert.That(_sandbox.DownloadCount, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Download_CorruptExistingCache_IsReplacedAfterValidation(bool batch) => Complete(Download_CorruptExistingCache_IsReplacedAfterValidationAsync(batch));

        private async Task Download_CorruptExistingCache_IsReplacedAfterValidationAsync(bool batch)
        {
            var info = _sandbox.File("Cube.ab", "good");
            await _sandbox.Initialize(info);
            System.IO.File.WriteAllText(_sandbox.PathFor(info), "evil");
            if (batch) Assert.That(await _sandbox.Updater.UpdateAsync(), Is.EqualTo(DLCUpdater.EResult.Success));
            else Assert.That(await _sandbox.Updater.DownloadAsync(info.resName), Is.True);
            Assert.That(_sandbox.DownloadCount, Is.EqualTo(1));
            Assert.That(System.IO.File.ReadAllText(_sandbox.PathFor(info)), Is.EqualTo("good"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Download_ValidCache_DoesNotIssueNetworkRequest(bool batch) => Complete(Download_ValidCache_DoesNotIssueNetworkRequestAsync(batch));

        private async Task Download_ValidCache_DoesNotIssueNetworkRequestAsync(bool batch)
        {
            var info = _sandbox.File("Cube.ab", "good");
            await _sandbox.Initialize(info);
            System.IO.File.WriteAllText(_sandbox.PathFor(info), "good");
            if (batch) Assert.That(await _sandbox.Updater.UpdateAsync(), Is.EqualTo(DLCUpdater.EResult.Success));
            else Assert.That(await _sandbox.Updater.DownloadAsync(info.resName), Is.True);
            Assert.That(_sandbox.DownloadCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator Download_ConcurrentOnDemandConsumers_ShareOneFileRequest() => AwaitCompletion(Download_ConcurrentConsumers_ShareOneFileRequestAsync(false));

        [UnityTest]
        public IEnumerator Download_ConcurrentBatchAndOnDemand_ShareOneFileRequest() => AwaitCompletion(Download_ConcurrentConsumers_ShareOneFileRequestAsync(true));

        private async Task Download_ConcurrentConsumers_ShareOneFileRequestAsync(bool includeBatch)
        {
            var info = _sandbox.File("Cube.ab", "good");
            await _sandbox.Initialize(info);
            _sandbox.DownloadGate = new TaskCompletionSource<bool>();
            var first = ToTask(_sandbox.Updater.DownloadAsync(info.resName));
            Task<DLCUpdater.EResult> batch = includeBatch ? ToTask(_sandbox.Updater.UpdateAsync()) : null;
            Task<bool> second = includeBatch ? null : ToTask(_sandbox.Updater.DownloadAsync(info.resName));
            var requestsBeforeCompletion = _sandbox.DownloadCount;
            _sandbox.DownloadGate.SetResult(true);
            Assert.That(await first, Is.True);
            if (includeBatch) Assert.That(await batch, Is.EqualTo(DLCUpdater.EResult.Success));
            else Assert.That(await second, Is.True);
            Assert.That(requestsBeforeCompletion, Is.EqualTo(1));
            Assert.That(_sandbox.DownloadCount, Is.EqualTo(1));
            Assert.That(_sandbox.Updater.Exists(info.resName), Is.True);
        }

        [Test]
        public void Download_Failure_IsNotCachedAndCanBeRetried() => Complete(Download_Failure_IsNotCachedAndCanBeRetriedAsync());

        private async Task Download_Failure_IsNotCachedAndCanBeRetriedAsync()
        {
            var info = _sandbox.File("Cube.ab", "good");
            await _sandbox.Initialize(info);
            _sandbox.DownloadGate = new TaskCompletionSource<bool>();
            _sandbox.DownloadGate.SetResult(false);
            Assert.That(await _sandbox.Updater.DownloadAsync(info.resName), Is.False);
            _sandbox.DownloadGate = null;
            Assert.That(await _sandbox.Updater.DownloadAsync(info.resName), Is.True);
            Assert.That(_sandbox.DownloadCount, Is.EqualTo(2));
        }

        [Test]
        public void Download_InvalidTransferredData_DoesNotOverwriteExistingTarget() => Complete(Download_InvalidTransferredData_DoesNotOverwriteExistingTargetAsync());

        private async Task Download_InvalidTransferredData_DoesNotOverwriteExistingTargetAsync()
        {
            var info = _sandbox.File("Cube.ab", "good");
            await _sandbox.Initialize(info);
            var target = _sandbox.PathFor(info);
            System.IO.File.WriteAllText(target, "old-invalid-content");
            _sandbox.DownloadContentOverride = "evil";
            Assert.That(await _sandbox.Updater.DownloadAsync(info.resName), Is.False);
            Assert.That(System.IO.File.ReadAllText(target), Is.EqualTo("old-invalid-content"));
            Assert.That(Directory.GetFiles(_sandbox.Root, "*.download"), Is.Empty);
        }

        [Test]
        public void Update_ManifestTransportThrows_PreservesOldManifest() => Complete(Update_ManifestTransportThrows_PreservesOldManifestAsync());

        private async Task Update_ManifestTransportThrows_PreservesOldManifestAsync()
        {
            await _sandbox.Initialize(_sandbox.File("Cube.ab", "v1"));
            var oldInfo = _sandbox.Updater.VersionInfo;
            var oldText = System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile);
            _sandbox.Uid = "v2";
            _sandbox.FailManifestRequest = true;
            Assert.That(await _sandbox.Updater.UpdateAsync(), Is.EqualTo(DLCUpdater.EResult.VersionError));
            Assert.That(_sandbox.Updater.VersionInfo, Is.SameAs(oldInfo));
            Assert.That(System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile), Is.EqualTo(oldText));
        }

        [UnityTest]
        public IEnumerator Download_DifferentResourceAliasesForSameHash_ShareOneFileRequest() => AwaitCompletion(Download_DifferentResourceAliasesForSameHash_ShareOneFileRequestAsync());

        private async Task Download_DifferentResourceAliasesForSameHash_ShareOneFileRequestAsync()
        {
            var info = _sandbox.File("Cube.ab", "good");
            var alias = _sandbox.File("Other.ab", "good");
            await _sandbox.Initialize(info, alias);
            _sandbox.DownloadGate = new TaskCompletionSource<bool>();
            var first = ToTask(_sandbox.Updater.DownloadAsync(info.resName));
            var second = ToTask(_sandbox.Updater.DownloadAsync(alias.resName));
            var requestsBeforeCompletion = _sandbox.DownloadCount;
            _sandbox.DownloadGate.SetResult(true);
            Assert.That(await first, Is.True);
            Assert.That(await second, Is.True);
            Assert.That(requestsBeforeCompletion, Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator Initialize_ConfigChangesDuringManifestRequest_RejectsStaleResponse() => AwaitCompletion(Initialize_ConfigChangesDuringManifestRequest_RejectsStaleResponseAsync());

        private async Task Initialize_ConfigChangesDuringManifestRequest_RejectsStaleResponseAsync()
        {
            await _sandbox.Initialize(_sandbox.File("Cube.ab", "v1"));
            var oldInfo = _sandbox.Updater.VersionInfo;
            var oldText = System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile);
            _sandbox.Uid = "v2";
            var response = ConfigHelper.ToJson(_sandbox.Manifest(_sandbox.File("Cube.ab", "v2")));
            _sandbox.ManifestGate = new TaskCompletionSource<string>();
            var initializing = ToTask(_sandbox.Updater.InitializeAsync());
            _sandbox.Uid = "v3";
            _sandbox.ManifestGate.SetResult(response);
            Assert.That(await initializing, Is.False);
            Assert.That(_sandbox.Updater.VersionInfo, Is.SameAs(oldInfo));
            Assert.That(System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile), Is.EqualTo(oldText));
        }

        [Test]
        public void Initialize_FailedVersionSwitch_PreservesOldResourceServerUrl() => Complete(Initialize_FailedVersionSwitch_PreservesOldResourceServerUrlAsync());

        private async Task Initialize_FailedVersionSwitch_PreservesOldResourceServerUrlAsync()
        {
            var info = _sandbox.File("Cube.ab", "good");
            await _sandbox.Initialize(info);
            var originalUrl = _sandbox.Updater.GetResServerUrl(info.resName);
            _sandbox.Uid = "v2";
            _sandbox.ServerUrl = "https://example.invalid/new-version";
            _sandbox.ManifestText = null;
            Assert.That(await _sandbox.Updater.InitializeAsync(), Is.False);
            Assert.That(_sandbox.Updater.GetResServerUrl(info.resName), Is.EqualTo(originalUrl));
        }

        [UnityTest]
        public IEnumerator Update_DuplicateHashAliasesAndOnDemand_ShareOneFileRequest() => AwaitCompletion(Update_DuplicateHashAliasesAndOnDemand_ShareOneFileRequestAsync());

        private async Task Update_DuplicateHashAliasesAndOnDemand_ShareOneFileRequestAsync()
        {
            var first = _sandbox.File("Cube.ab", "good");
            var second = _sandbox.File("Other.ab", "good");
            await _sandbox.Initialize(first, second);
            _sandbox.DownloadGate = new TaskCompletionSource<bool>();
            var updating = ToTask(_sandbox.Updater.UpdateAsync());
            var downloading = ToTask(_sandbox.Updater.DownloadAsync(second.resName));
            var requestCount = _sandbox.DownloadCount;
            _sandbox.DownloadGate.SetResult(true);
            Assert.That(await updating, Is.EqualTo(DLCUpdater.EResult.Success));
            Assert.That(await downloading, Is.True);
            Assert.That(requestCount, Is.EqualTo(1));
            Assert.That(_sandbox.DownloadCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_CorruptDownload_ReturnsDownloadError() => Complete(Update_CorruptDownload_ReturnsDownloadErrorAsync());

        private async Task Update_CorruptDownload_ReturnsDownloadErrorAsync()
        {
            await _sandbox.Initialize(_sandbox.File("Cube.ab", "good"));
            _sandbox.DownloadContentOverride = "evil";
            Assert.That(await _sandbox.Updater.UpdateAsync(), Is.EqualTo(DLCUpdater.EResult.DownloadError));
            Assert.That(Directory.GetFiles(_sandbox.Root, "*.download"), Is.Empty);
        }
        [Test]
        public void Initialize_ManifestCommitFails_PreservesOldMemoryAndDisk() => Complete(Initialize_ManifestCommitFails_PreservesOldMemoryAndDiskAsync());

        private async Task Initialize_ManifestCommitFails_PreservesOldMemoryAndDiskAsync()
        {
            if (Path.DirectorySeparatorChar != '\\') Assert.Ignore("Requires Windows file sharing semantics.");
            await _sandbox.Initialize(_sandbox.File("Cube.ab", "v1"));
            var oldInfo = _sandbox.Updater.VersionInfo;
            var oldText = System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile);
            _sandbox.Uid = "v2";
            _sandbox.ManifestText = ConfigHelper.ToJson(_sandbox.Manifest(_sandbox.File("Cube.ab", "v2")));
            using (System.IO.File.Open(_sandbox.Updater.DLCVersionInfoLocalFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Assert.That(await _sandbox.Updater.InitializeAsync(), Is.False);
            }
            Assert.That(_sandbox.Updater.VersionInfo, Is.SameAs(oldInfo));
            Assert.That(System.IO.File.ReadAllText(_sandbox.Updater.DLCVersionInfoLocalFile), Is.EqualTo(oldText));
            Assert.That(Directory.GetFiles(_sandbox.Root, "*.download"), Is.Empty);
        }
        [UnityTest]
        public IEnumerator Download_ParallelLimitOne_QueuesDistinctFiles() => AwaitCompletion(Download_ParallelLimitOne_QueuesDistinctFilesAsync());

        private async Task Download_ParallelLimitOne_QueuesDistinctFilesAsync()
        {
            _sandbox.Dispose();
            _sandbox = new Sandbox(1);
            var firstInfo = _sandbox.File("Cube.ab", "first");
            var secondInfo = _sandbox.File("Other.ab", "second");
            await _sandbox.Initialize(firstInfo, secondInfo);
            _sandbox.DownloadGate = new TaskCompletionSource<bool>();
            var first = ToTask(_sandbox.Updater.DownloadAsync(firstInfo.resName));
            var second = ToTask(_sandbox.Updater.DownloadAsync(secondInfo.resName));
            var requestsBeforeRelease = _sandbox.DownloadCount;
            _sandbox.DownloadGate.SetResult(true);
            Assert.That(await first, Is.True);
            Assert.That(await second, Is.True);
            Assert.That(requestsBeforeRelease, Is.EqualTo(1));
            Assert.That(_sandbox.DownloadCount, Is.EqualTo(2));
        }

        private static IEnumerator AwaitCompletion(Task task)
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (!task.IsCompleted)
            {
                Assert.That(DateTime.UtcNow, Is.LessThan(deadline), "The isolated operation did not complete.");
                yield return null;
            }
            task.GetAwaiter().GetResult();
        }
        // These fakes complete inline; fail clearly instead of blocking Unity on a delayed task.
        private static void Complete(Task task)
        {
            Assert.That(task.IsCompleted, Is.True, "The isolated test must complete synchronously on the Unity thread.");
            task.GetAwaiter().GetResult();
        }

        private static async Task<T> ToTask<T>(ETask<T> task) => await task;

        // Expected network/manifest errors are outcomes under test, not Unity console failures.
        private sealed class TestDebugger : IDebugger
        {
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public void LogException(Exception exception) { }
        }

        private sealed class Sandbox : IDisposable
        {
            public readonly string Parent = Path.Combine(Path.GetTempPath(), "EasyFramework-DLCUpdaterTests-" + Guid.NewGuid().ToString("N"));
            public readonly string Root;
            public readonly DLCUpdater Updater;
            public string Uid = "v1";
            public string ServerUrl = "https://example.invalid/dlc";
            public TaskCompletionSource<string> ManifestGate;
            public string ManifestText;
            public int DownloadCount;
            public TaskCompletionSource<bool> DownloadGate;
            public string DownloadContentOverride;
            public bool FailManifestRequest;
            private readonly Dictionary<string, string> _contents = new Dictionary<string, string>();

            public Sandbox(int parallel = 3)
            {
                Root = Path.Combine(Parent, "DLC");
                Directory.CreateDirectory(Root);
                Updater = new TestUpdater(this, parallel);
            }

            public HashFileInfo File(string resource, string content)
            {
                string hash;
                using (var md5 = MD5.Create())
                    hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-", "").ToLowerInvariant();
                var info = new HashFileInfo { resName = resource, fileName = hash + Path.GetExtension(resource), length = Encoding.UTF8.GetByteCount(content) };
                _contents[info.fileName] = content;
                return info;
            }

            public DLCVersionInfo Manifest(params HashFileInfo[] files) => new DLCVersionInfo { uid = Uid, hashFiles = files };
            public string PathFor(HashFileInfo file) => Path.Combine(Root, file.fileName);

            public async Task Initialize(params HashFileInfo[] files)
            {
                ManifestText = ConfigHelper.ToJson(Manifest(files));
                Assert.That(await Updater.InitializeAsync(), Is.True);
            }

            private async ETask<string> FetchManifest(string url)
            {
                if (FailManifestRequest) throw new IOException("Simulated manifest transport failure.");
                if (ManifestGate != null) return await ManifestGate.Task;
                return ManifestText;
            }

            private async ETask<bool> Download(string url, string file)
            {
                DownloadCount++;
                if (DownloadGate != null && !await DownloadGate.Task) return false;
                var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
                var content = _contents[fileName];
                System.IO.File.WriteAllText(file, DownloadContentOverride ?? content);
                return true;
            }

            private sealed class TestUpdater : DLCUpdater
            {
                private readonly Sandbox _sandbox;

                public TestUpdater(Sandbox sandbox, int parallel) : base(sandbox.Root, parallel)
                {
                    _sandbox = sandbox;
                }

                protected override string VersionUid => _sandbox.Uid;
                protected override string ServerUrl => _sandbox.ServerUrl;
                protected override ETask<string> LoadTextAsync(string url) => _sandbox.FetchManifest(url);
                protected override ETask<bool> DownloadFileAsync(string url, string file) => _sandbox.Download(url, file);
            }

            public void Dispose()
            {
                if (Directory.Exists(Parent)) Directory.Delete(Parent, true);
            }
        }
    }
}
