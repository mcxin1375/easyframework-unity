using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace EasyFramework.Tests
{
    public class DownloadTaskTests
    {
        private string _directory;
        private string _source;
        private string _file;
        private int _maxRetryCount;
        private static readonly MethodInfo Tick = typeof(ETask).GetMethod("Tick", BindingFlags.Static | BindingFlags.NonPublic);

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "EasyFrameworkDownloadTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _source = Path.Combine(_directory, "source.bin");
            _file = Path.Combine(_directory, "target.bin");
            File.WriteAllText(_source, "new content");
            File.WriteAllText(_file, "old content");
            _maxRetryCount = EasyFrameworkSettings.Instance.maxRetryCount;
            EasyFrameworkSettings.Instance.maxRetryCount = 1;
        }

        [TearDown]
        public void TearDown()
        {
            EasyFrameworkSettings.Instance.maxRetryCount = _maxRetryCount;
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [UnityTest]
        public IEnumerator Download_ValidatedFile_ReplacesExistingTarget()
        {
            var receiver = new Receiver();
            bool success = false;
            yield return Run(ETask.UnityWebRequestDownload(new Uri(_source).AbsoluteUri, _file, receiver,
                requestIndex: 7, length: new FileInfo(_source).Length, md5: MD5Helper.MD5File(_source)), value => success = value);
            Assert.That(success, Is.True);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("new content"));
            Assert.That(receiver.Index, Is.EqualTo(7));
            Assert.That(Directory.GetFiles(_directory, "*.download"), Is.Empty);
        }

        [UnityTest]
        public IEnumerator Download_InvalidLength_PreservesExistingTarget()
        {
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Downloaded file validation failed"));
            bool success = true;
            yield return Run(ETask.UnityWebRequestDownload(new Uri(_source).AbsoluteUri, _file, length: 1), value => success = value);
            Assert.That(success, Is.False);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old content"));
            Assert.That(Directory.GetFiles(_directory, "*.download"), Is.Empty);
        }

        [UnityTest]
        public IEnumerator Download_InvalidMD5_PreservesExistingTarget()
        {
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Downloaded file validation failed"));
            bool success = true;
            yield return Run(ETask.UnityWebRequestDownload(new Uri(_source).AbsoluteUri, _file,
                length: new FileInfo(_source).Length, md5: new string('0', 32)), value => success = value);
            Assert.That(success, Is.False);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old content"));
            Assert.That(Directory.GetFiles(_directory, "*.download"), Is.Empty);
        }

        [UnityTest]
        public IEnumerator Download_CancelledRequest_PreservesExistingTarget()
        {
            using var cancellation = new CancellationTokenSource();
            var task = ETask.UnityWebRequestDownload(new Uri(_source).AbsoluteUri, _file, cancellationToken: cancellation.Token);
            cancellation.Cancel();
            bool success = true;
            yield return Run(task, value => success = value);
            Assert.That(success, Is.False);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old content"));
            Assert.That(Directory.GetFiles(_directory, "*.download"), Is.Empty);
        }

        [UnityTest]
        public IEnumerator Download_PreCancelledRequest_DoesNotCreateDirectory()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var file = Path.Combine(_directory, "not-created", "target.bin");
            bool success = true;
            yield return Run(ETask.UnityWebRequestDownload(new Uri(_source).AbsoluteUri, file,
                cancellationToken: cancellation.Token), value => success = value);
            Assert.That(success, Is.False);
            Assert.That(Directory.Exists(Path.GetDirectoryName(file)), Is.False);
        }

        [UnityTest]
        public IEnumerator DLCDownloader_InvalidHashFails_AndCanRetry()
        {
            var md5 = MD5Helper.MD5File(_source);
            var downloader = new DLCDownloader();
            downloader.AddRequest(new Uri(_source).AbsoluteUri, _file, new FileInfo(_source).Length, md5);
            File.WriteAllText(_source, "bad content");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Downloaded file validation failed"));
            bool success = true;
            yield return Run(downloader.StartAsync(), value => success = value);
            Assert.That(success, Is.False);
            Assert.That(downloader.CompletedCount, Is.Zero);
            Assert.That(File.ReadAllText(_file), Is.EqualTo("old content"));

            File.WriteAllText(_source, "new content");
            yield return Run(downloader.StartAsync(), value => success = value);
            Assert.That(success, Is.True);
            Assert.That(downloader.CompletedCount, Is.EqualTo(1));
            Assert.That(downloader.Progress, Is.EqualTo(1));
            Assert.That(File.ReadAllText(_file), Is.EqualTo("new content"));
            Assert.That(Directory.GetFiles(_directory, "*.download"), Is.Empty);
        }

        private static IEnumerator Run(ETask<bool> task, Action<bool> result)
        {
            bool completed = false;
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                result(awaiter.GetResult());
                yield break;
            }
            awaiter.OnCompleted(() =>
            {
                result(awaiter.GetResult());
                completed = true;
            });
            float deadline = Time.realtimeSinceStartup + 10;
            while (!completed && Time.realtimeSinceStartup < deadline)
            {
                Tick.Invoke(null, null);
                yield return null;
            }
            Assert.That(completed, Is.True, "Download did not finish.");
        }

        private static IEnumerator Run(ETask<UnityWebRequest> task, Action<bool> result)
        {
            bool completed = false;
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                var request = awaiter.GetResult();
                result(request != null && request.result == UnityWebRequest.Result.Success);
                completed = true;
            });
            float deadline = Time.realtimeSinceStartup + 10;
            while (!completed && Time.realtimeSinceStartup < deadline)
            {
                Tick.Invoke(null, null);
                yield return null;
            }
            Assert.That(completed, Is.True, "Download did not finish.");
        }

        private sealed class Receiver : IHttpReceiver
        {
            public int Index = -1;
            public void OnProgress(int requestIndex, ulong bytesReceived) => Index = requestIndex;
        }
    }
}
