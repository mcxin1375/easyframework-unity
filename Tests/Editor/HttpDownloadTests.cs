using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EasyFramework.Tests
{
    public class HttpDownloadTests
    {
        private string _directory;
        private string _file;
        private IDebugger _debugger;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "EasyFrameworkHttpTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _file = Path.Combine(_directory, "asset.bin");
            _debugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
        }

        [TearDown]
        public void TearDown()
        {
            FDebug.Debugger = _debugger;
            Directory.Delete(_directory, true);
        }

        [UnityTest]
        public IEnumerator Download_Success_ReplacesOldFileAndDisposesStream()
        {
            File.WriteAllText(_file, "old");
            var stream = new DownloadStream();
            yield return Download(stream, 3, true);
            Assert.That(File.ReadAllBytes(_file), Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(stream.Disposed, Is.True);
            AssertNoTempFiles();
        }

        [UnityTest]
        public IEnumerator Download_UnknownLength_CreatesFinalFile()
        {
            yield return Download(new DownloadStream(), null, true);
            Assert.That(File.ReadAllBytes(_file), Is.EqualTo(new byte[] { 1, 2, 3 }));
            AssertNoTempFiles();
        }

        [UnityTest]
        public IEnumerator Download_InterruptedStream_PreservesOldFile()
        {
            File.WriteAllText(_file, "old");
            yield return Download(new DownloadStream { Interrupt = true }, 6, false);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old"));
            AssertNoTempFiles();
        }

        [UnityTest]
        public IEnumerator Download_InterruptedStream_DoesNotCreateFinalFile()
        {
            yield return Download(new DownloadStream { Interrupt = true }, 6, false);
            Assert.That(File.Exists(_file), Is.False);
            AssertNoTempFiles();
        }

        [UnityTest]
        public IEnumerator Download_LengthMismatch_PreservesOldFile()
        {
            File.WriteAllText(_file, "old");
            yield return Download(new DownloadStream(), 10, false);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old"));
            AssertNoTempFiles();
        }

        [UnityTest]
        public IEnumerator Download_HttpFailure_PreservesOldFile()
        {
            File.WriteAllText(_file, "old");
            yield return Download(new DownloadStream(), 3, false, status: HttpStatusCode.NotFound);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old"));
            AssertNoTempFiles();
        }

        [UnityTest]
        public IEnumerator Download_CancelledDuringRead_PreservesOldFile()
        {
            File.WriteAllText(_file, "old");
            using var cancellation = new CancellationTokenSource();
            yield return Download(new DownloadStream { AfterRead = cancellation.Cancel }, 3, false, cancellation.Token);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old"));
            AssertNoTempFiles();
        }

        [UnityTest]
        public IEnumerator Download_CancelledBeforeCommit_PreservesOldFile()
        {
            File.WriteAllText(_file, "old");
            using var cancellation = new CancellationTokenSource();
            yield return Download(new DownloadStream { OnDispose = cancellation.Cancel }, 3, false, cancellation.Token);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old"));
            AssertNoTempFiles();
        }

        [Test]
        public void CreateDownloadTempPath_IsUniqueAndNextToNormalizedTarget()
        {
            string file = Path.Combine(_directory, "nested", "asset.bin");
            string first = FileHelper.CreateDownloadTempPath(file);
            string second = FileHelper.CreateDownloadTempPath(file);
            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(Path.GetDirectoryName(first), Is.EqualTo(Path.GetDirectoryName(Path.GetFullPath(file))));
            Assert.That(Directory.Exists(Path.GetDirectoryName(first)), Is.True);
            Assert.That(first, Does.EndWith(".download"));
        }

        [Test]
        public void VerifyFileIntegrity_ChecksExistenceLengthAndHash()
        {
            Assert.That(FileHelper.VerifyFileIntegrity(_file), Is.False);
            File.WriteAllText(_file, "abc");
            string hash = MD5Helper.MD5File(_file);
            Assert.That(FileHelper.VerifyFileIntegrity(_file, 3, hash.ToLowerInvariant()), Is.True);
            Assert.That(FileHelper.VerifyFileIntegrity(_file, 4, hash), Is.False);
            Assert.That(FileHelper.VerifyFileIntegrity(_file, 3, new string('0', 32)), Is.False);
        }

        [TestCase("d41d8cd98f00b204e9800998ecf8427e.bundle", "d41d8cd98f00b204e9800998ecf8427e")]
        [TestCase("D41D8CD98F00B204E9800998ECF8427E.bundle", "D41D8CD98F00B204E9800998ECF8427E")]
        [TestCase("asset.bundle", null)]
        [TestCase("g41d8cd98f00b204e9800998ecf8427e.bundle", null)]
        [TestCase("prefix_d41d8cd98f00b204e9800998ecf8427e.bundle", null)]
        public void ExtractMD5FromFileName_OnlyAcceptsExactHexStem(string name, string expected)
        {
            Assert.That(FileHelper.ExtractMD5FromFileName(Path.Combine(_directory, name)), Is.EqualTo(expected));
        }

        [Test]
        public void ReplaceFileFromTemp_Failure_PreservesOldTarget()
        {
            File.WriteAllText(_file, "old");
            Assert.Throws<FileNotFoundException>(() => FileHelper.ReplaceFileFromTemp(Path.Combine(_directory, "missing.download"), _file));
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old"));
        }

        private IEnumerator Download(DownloadStream stream, long? length, bool expected, CancellationToken token = default, HttpStatusCode status = HttpStatusCode.OK)
        {
            using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentLength = length;
            using var client = new HttpClient(new ResponseHandler(response));
            bool done = false;
            bool result = !expected;
            ObserveDownload(client, token, value => { result = value; done = true; }).Forget();
            DateTime deadline = DateTime.UtcNow.AddSeconds(10);
            while (!done)
            {
                Assert.That(DateTime.UtcNow, Is.LessThan(deadline), "Download did not complete");
                yield return null;
            }
            Assert.That(result, Is.EqualTo(expected));
        }

        private async EVoid ObserveDownload(HttpClient client, CancellationToken token, Action<bool> completed)
        {
            completed(await client.DownloadAsync("https://test.invalid/asset", _file, token: token));
        }

        private void AssertNoTempFiles() => Assert.That(Directory.GetFiles(_directory, "*.download", SearchOption.AllDirectories), Is.Empty);

        private sealed class ResponseHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public ResponseHandler(HttpResponseMessage response) => _response = response;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_response);
        }

        private sealed class DownloadStream : MemoryStream
        {
            public bool Interrupt;
            public bool Disposed;
            public Action AfterRead;
            public Action OnDispose;
            public override bool CanSeek => false;
            public DownloadStream() : base(new byte[] { 1, 2, 3 }) { }
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Interrupt && Position > 0) throw new IOException("Interrupted response");
                int read = Read(buffer, offset, count);
                if (read > 0) AfterRead?.Invoke();
                return Task.FromResult(read);
            }
            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                OnDispose?.Invoke();
                base.Dispose(disposing);
            }
        }

        private sealed class QuietDebugger : IDebugger
        {
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public void LogException(Exception exception) { }
        }
    }
}
