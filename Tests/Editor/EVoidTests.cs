using System;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class EVoidTests
    {
        [Test]
        public void CompletedAwait_RunsSynchronously()
        {
            var invoked = false;

            InvokeAfterAwait(ETask.CompletedTask, () => invoked = true).Forget();

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void PendingAwait_ReturnsBeforeCompletion()
        {
            var source = new ManualAwaiter();
            var task = new ETask(source, source.Token);
            var invoked = false;

            InvokeAfterAwait(task, () => invoked = true).Forget();

            Assert.That(invoked, Is.False);

            source.SetResult(source.Token);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void Exception_IsPublishedAsUnobservedException()
        {
            Exception observed = null;
            Action<Exception> handler = exception => observed = exception;
            EVoidScheduler.UnobservedTaskException += handler;

            try
            {
                ThrowAsync().Forget();
            }
            finally
            {
                EVoidScheduler.UnobservedTaskException -= handler;
            }

            Assert.That(observed, Is.TypeOf<InvalidOperationException>());
        }

        private static async EVoid InvokeAfterAwait(ETask task, Action callback)
        {
            await task;
            callback();
        }

        private static async EVoid ThrowAsync()
        {
            await ETask.CompletedTask;
            throw new InvalidOperationException("EVoid test exception.");
        }

        private sealed class ManualAwaiter : IETaskAwaiter
        {
            private Action<object> _continuation;
            private object _continuationState;
            private ETaskStatus _status = ETaskStatus.Pending;

            public Guid Token { get; } = Guid.NewGuid();

            public ETaskStatus GetStatus(Guid token)
            {
                ValidateToken(token);
                return _status;
            }

            public void OnCompleted(Action<object> continuation, object state, Guid token)
            {
                ValidateToken(token);
                _continuation = continuation;
                _continuationState = state;

                if (_status != ETaskStatus.Pending)
                {
                    InvokeContinuation();
                }
            }

            public void SetException(Guid token, Exception exception)
            {
                ValidateToken(token);
                _status = ETaskStatus.Faulted;
                InvokeContinuation();
            }

            public void SetResult(Guid token)
            {
                ValidateToken(token);
                _status = ETaskStatus.Succeeded;
                InvokeContinuation();
            }

            public void GetResult(Guid token)
            {
                ValidateToken(token);
            }

            private void InvokeContinuation()
            {
                var continuation = _continuation;
                var state = _continuationState;
                _continuation = null;
                _continuationState = null;
                continuation?.Invoke(state);
            }

            private void ValidateToken(Guid token)
            {
                if (token != Token) throw new InvalidOperationException("Unexpected ETask token.");
            }
        }
    }
}
