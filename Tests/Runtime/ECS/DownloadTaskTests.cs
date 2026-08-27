using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class DownloadTaskTests
    {
        [Test]
        public void EmptyTaskCompletesWithZeroProgress()
        {
            var task = new DownloadTask();
            var result = task.StartAsync().GetAwaiter().GetResult();

            Assert.That(result, Is.True);
            Assert.That(task.TotalCount, Is.EqualTo(0));
            Assert.That(task.CompletedCount, Is.EqualTo(0));
            Assert.That(task.Progress, Is.EqualTo(0));
        }

        [Test]
        public void RequestDataIsPublicAndProgressUsesDeclaredBytes()
        {
            var task = new DownloadTask();
            task.AddRequest("url", "file", 100);

            Assert.That(task.TotalCount, Is.EqualTo(1));
            Assert.That(task.TotalBytes, Is.EqualTo(100));
            Assert.That(task.Progress, Is.EqualTo(0));
        }

        [Test]
        public void AddRequestIsLockedAfterStart()
        {
            var task = new DownloadTask();
            task.StartAsync();

            Assert.Throws<System.InvalidOperationException>(() => task.AddRequest("url", "file"));
        }
    }
}
