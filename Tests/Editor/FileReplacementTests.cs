using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class FileReplacementTests
    {
        private string _root;
        private readonly List<string> _temporaryPaths = new();

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "EasyFrameworkFileReplacement-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            _temporaryPaths.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Test]
        public void TemporaryPath_IsUniqueAndBesideItsDestination()
        {
            var target = Path.Combine(_root, "new-parent", "LatestVersion.json");
            var first = FileHelper.CreateTempPath(target);
            var second = FileHelper.CreateTempPath(target);

            Assert.That(Path.GetDirectoryName(first), Is.EqualTo(Path.GetDirectoryName(target)));
            Assert.That(Path.GetDirectoryName(second), Is.EqualTo(Path.GetDirectoryName(target)));
            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(File.Exists(first), Is.False);
            Assert.That(Directory.Exists(first), Is.False);
            Assert.That(File.Exists(target), Is.False);
        }

        [Test]
        public void PreparedDirectoryAndFile_ArePublishedTogetherWithoutKeepingBackups()
        {
            var directory = CreateDirectory("bundles", "old.ab", "old bundle");
            var file = CreateFile("LatestVersion.json", "old version");
            var newDirectory = PrepareDirectory(directory, "new.ab", "new bundle");
            var newFile = PrepareFile(file, "new version");

            Assert.That(File.ReadAllText(Path.Combine(directory, "old.ab")), Is.EqualTo("old bundle"));
            Assert.That(File.ReadAllText(file), Is.EqualTo("old version"));
            FileHelper.ReplacePathsFromTemp(new[] { (newDirectory, directory), (newFile, file) });

            Assert.That(File.ReadAllText(Path.Combine(directory, "new.ab")), Is.EqualTo("new bundle"));
            Assert.That(File.Exists(Path.Combine(directory, "old.ab")), Is.False);
            Assert.That(File.ReadAllText(file), Is.EqualTo("new version"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(directory, file);
        }

        [Test]
        public void NewDirectoryAndFile_CanBePublishedWithoutPreviousOutputs()
        {
            var directory = Path.Combine(_root, "bundles");
            var file = Path.Combine(_root, "LatestVersion.json");
            var newDirectory = PrepareDirectory(directory, "new.ab", "new bundle");
            var newFile = PrepareFile(file, "new version");

            FileHelper.ReplacePathsFromTemp(new[] { (newDirectory, directory), (newFile, file) });

            Assert.That(File.ReadAllText(Path.Combine(directory, "new.ab")), Is.EqualTo("new bundle"));
            Assert.That(File.ReadAllText(file), Is.EqualTo("new version"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(directory, file);
        }

        [Test]
        [Platform("Win")]
        public void LockedLaterFile_RestoresPreviouslyReplacedDirectoryAndMetadata()
        {
            var directory = CreateDirectory("bundles", "old.ab", "old bundle");
            var history = CreateFile("DLCBuilderVersionList.json", "old history");
            var latest = CreateFile("LatestVersion.json", "old version");
            var newDirectory = PrepareDirectory(directory, "new.ab", "new bundle");
            var newHistory = PrepareFile(history, "new history");
            var newLatest = PrepareFile(latest, "new version");

            using (new FileStream(latest, FileMode.Open, FileAccess.Read, FileShare.Read))
                Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[]
                {
                    (newDirectory, directory), (newHistory, history), (newLatest, latest)
                }));

            Assert.That(File.ReadAllText(Path.Combine(directory, "old.ab")), Is.EqualTo("old bundle"));
            Assert.That(File.Exists(Path.Combine(directory, "new.ab")), Is.False);
            Assert.That(File.ReadAllText(history), Is.EqualTo("old history"));
            Assert.That(File.ReadAllText(latest), Is.EqualTo("old version"));
            Assert.That(File.Exists(newLatest), Is.True, "The caller still owns a temporary file that was never installed.");
            AssertOnlyTargetsAndOwnedTemporaryPaths(directory, history, latest);
        }

        [Test]
        [Platform("Win")]
        public void FailedGroup_RemovesNewOutputThatHadNoPreviousVersion()
        {
            var directory = Path.Combine(_root, "new-version");
            var latest = CreateFile("LatestVersion.json", "old version");
            var newDirectory = PrepareDirectory(directory, "new.ab", "new bundle");
            var newLatest = PrepareFile(latest, "new version");

            using (new FileStream(latest, FileMode.Open, FileAccess.Read, FileShare.Read))
                Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[] { (newDirectory, directory), (newLatest, latest) }));

            Assert.That(Directory.Exists(directory), Is.False);
            Assert.That(File.ReadAllText(latest), Is.EqualTo("old version"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(latest);
        }

        [Test]
        public void MissingTemporaryOutput_IsRejectedBeforeReplacingOtherOutputs()
        {
            var first = CreateFile("first.json", "old first");
            var second = CreateFile("second.json", "old second");
            var newFirst = PrepareFile(first, "new first");
            var missing = FileHelper.CreateTempPath(second);

            Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[] { (newFirst, first), (missing, second) }));

            Assert.That(File.ReadAllText(first), Is.EqualTo("old first"));
            Assert.That(File.ReadAllText(second), Is.EqualTo("old second"));
            Assert.That(File.ReadAllText(newFirst), Is.EqualTo("new first"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(first, second);
        }

        [Test]
        public void FileCannotReplaceDirectory_AndItsExistingContentsSurvive()
        {
            var directory = CreateDirectory("LatestVersion.json", "business.txt", "preserve");
            var temporary = PrepareFile(directory, "new version");

            Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[] { (temporary, directory) }));

            Assert.That(File.ReadAllText(Path.Combine(directory, "business.txt")), Is.EqualTo("preserve"));
            Assert.That(File.ReadAllText(temporary), Is.EqualTo("new version"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(directory);
        }

        [Test]
        public void DirectoryCannotReplaceFile_AndItsExistingContentsSurvive()
        {
            var file = CreateFile("bundles", "preserve");
            var temporary = PrepareDirectory(file, "new.ab", "new bundle");

            Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[] { (temporary, file) }));

            Assert.That(File.ReadAllText(file), Is.EqualTo("preserve"));
            Assert.That(File.ReadAllText(Path.Combine(temporary, "new.ab")), Is.EqualTo("new bundle"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(file);
        }

        [Test]
        public void DuplicateTargets_AreRejectedBeforeAnyReplacement()
        {
            var file = CreateFile("LatestVersion.json", "old version");
            var first = PrepareFile(file, "first version");
            var second = PrepareFile(file, "second version");

            Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[] { (first, file), (second, file) }));

            Assert.That(File.ReadAllText(file), Is.EqualTo("old version"));
            Assert.That(File.ReadAllText(first), Is.EqualTo("first version"));
            Assert.That(File.ReadAllText(second), Is.EqualTo("second version"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(file);
        }

        [Test]
        [Platform("Win")]
        public void SeparateSuccessfulCalls_AreUnaffectedByALaterFailedCall()
        {
            var bundle = CreateFile("bundle.ab", "old bundle");
            FileHelper.ReplacePathsFromTemp(new[] { (PrepareFile(bundle, "first bundle"), bundle) });
            FileHelper.ReplacePathsFromTemp(new[] { (PrepareFile(bundle, "second bundle"), bundle) });
            var latest = CreateFile("LatestVersion.json", "old version");
            var newLatest = PrepareFile(latest, "new version");

            using (new FileStream(latest, FileMode.Open, FileAccess.Read, FileShare.Read))
                Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[] { (newLatest, latest) }));

            Assert.That(File.ReadAllText(bundle), Is.EqualTo("second bundle"));
            Assert.That(File.ReadAllText(latest), Is.EqualTo("old version"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(bundle, latest);
        }

        [Test]
        public void SuccessfulPublication_RemovesExplicitlySelectedHistoricalDirectories()
        {
            var latest = CreateFile("LatestVersion.json", "old version");
            var historical = CreateDirectory("old-version", "bundle.ab", "old bundle");
            var newLatest = PrepareFile(latest, "new version");

            FileHelper.ReplacePathsFromTemp(new[] { (newLatest, latest) }, new[] { historical });

            Assert.That(File.ReadAllText(latest), Is.EqualTo("new version"));
            Assert.That(Directory.Exists(historical), Is.False);
            AssertOnlyTargetsAndOwnedTemporaryPaths(latest);
        }

        [Test]
        [Platform("Win")]
        public void LockedHistoricalDirectory_RestoresEarlierRemovalAndPublication()
        {
            var latest = CreateFile("LatestVersion.json", "old version");
            var historical = CreateDirectory("old-version", "bundle.ab", "old bundle");
            var locked = CreateDirectory("locked-version", "bundle.ab", "locked bundle");
            var newLatest = PrepareFile(latest, "new version");

            // A directory handle without FILE_SHARE_DELETE makes its rename fail after the preceding removals.
            using (var directoryHandle = CreateFileW(locked, 0x80000000, 3, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero))
            {
                Assert.That(directoryHandle.IsInvalid, Is.False, "Could not lock historical directory: " + Marshal.GetLastWin32Error());
                Assert.Throws<IOException>(() => FileHelper.ReplacePathsFromTemp(new[] { (newLatest, latest) }, new[] { historical, locked }));
            }

            Assert.That(File.ReadAllText(latest), Is.EqualTo("old version"));
            Assert.That(File.ReadAllText(Path.Combine(historical, "bundle.ab")), Is.EqualTo("old bundle"));
            Assert.That(File.ReadAllText(Path.Combine(locked, "bundle.ab")), Is.EqualTo("locked bundle"));
            AssertOnlyTargetsAndOwnedTemporaryPaths(latest, historical, locked);
        }

        private string CreateDirectory(string name, string file, string content)
        {
            var directory = Path.Combine(_root, name);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, file), content);
            return directory;
        }

        private string CreateFile(string name, string content)
        {
            var file = Path.Combine(_root, name);
            File.WriteAllText(file, content);
            return file;
        }

        private string PrepareFile(string target, string content)
        {
            var temporary = FileHelper.CreateTempPath(target);
            _temporaryPaths.Add(temporary);
            File.WriteAllText(temporary, content);
            return temporary;
        }

        private string PrepareDirectory(string target, string file, string content)
        {
            var temporary = FileHelper.CreateTempPath(target);
            _temporaryPaths.Add(temporary);
            Directory.CreateDirectory(temporary);
            File.WriteAllText(Path.Combine(temporary, file), content);
            return temporary;
        }

        private void AssertOnlyTargetsAndOwnedTemporaryPaths(params string[] targets)
        {
            var expected = targets.Concat(_temporaryPaths).Where(path => File.Exists(path) || Directory.Exists(path)).ToArray();
            CollectionAssert.AreEquivalent(expected, Directory.GetFileSystemEntries(_root), "Restored or successful replacement must not retain backup paths.");
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileW(string fileName, uint desiredAccess, uint shareMode,
            IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);
    }
}
