using System;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class ETaskCoroutineTests
    {
        [Test]
        public void Coroutine_CompletedTasks_StartWithoutThrowing()
        {
            Assert.DoesNotThrow(() => ETask.CompletedTask.Forget());
            Assert.DoesNotThrow(() => ETask.FromResult(42).Forget());
        }

        [Test]
        public void Coroutine_IncompleteTask_RegistersContinuationAndReturns()
        {
            Guid token = Guid.NewGuid();
            var source = new ManualAwaiter(token);
            var task = new ETask(source, token);

            task.Forget();

            Assert.That(source.ContinuationCount, Is.EqualTo(1));
            Assert.That(source.ContinuationInvoked, Is.False);

            source.SetResult(token);

            Assert.That(source.ContinuationInvoked, Is.True);
        }

        [Test]
        public void Coroutine_IncompleteGenericTask_RegistersContinuationAndReturns()
        {
            Guid token = Guid.NewGuid();
            var source = new ManualAwaiter<int>(token);
            var task = new ETask<int>(source, token);

            task.Forget();

            Assert.That(source.ContinuationCount, Is.EqualTo(1));
            Assert.That(source.ContinuationInvoked, Is.False);

            source.SetResult(42, token);

            Assert.That(source.ContinuationInvoked, Is.True);
        }

        private sealed class ManualAwaiter : IETaskAwaiter
        {
            private readonly Guid _token;
            private Action _continuation;
            private ETaskStatus _status = ETaskStatus.Pending;

            public ManualAwaiter(Guid token)
            {
                _token = token;
            }

            public int ContinuationCount { get; private set; }
            public bool ContinuationInvoked { get; private set; }

            public ETaskStatus GetStatus(Guid token)
            {
                ValidateToken(token);
                return _status;
            }

            public void OnCompleted(Action continuation, Guid token)
            {
                ValidateToken(token);
                ContinuationCount++;

                if (_status != ETaskStatus.Pending)
                {
                    InvokeContinuation(continuation);
                    return;
                }

                _continuation = continuation;
            }

            public void SetResult(Guid token)
            {
                ValidateToken(token);
                _status = ETaskStatus.Succeeded;
                InvokeStoredContinuation();
            }

            public void SetException(Guid token, Exception exception)
            {
                ValidateToken(token);
                _status = ETaskStatus.Faulted;
                InvokeStoredContinuation();
            }

            public void GetResult(Guid token)
            {
                ValidateToken(token);
            }

            private void InvokeStoredContinuation()
            {
                Action continuation = _continuation;
                _continuation = null;
                InvokeContinuation(continuation);
            }

            private void InvokeContinuation(Action continuation)
            {
                ContinuationInvoked = true;
                continuation?.Invoke();
            }

            private void ValidateToken(Guid token)
            {
                if (token != _token)
                {
                    throw new InvalidOperationException("Unexpected ETask token.");
                }
            }
        }

        private sealed class ManualAwaiter<T> : IETaskAwaiter<T>
        {
            private readonly Guid _token;
            private Action _continuation;
            private ETaskStatus _status = ETaskStatus.Pending;
            private T _result;

            public ManualAwaiter(Guid token)
            {
                _token = token;
            }

            public int ContinuationCount { get; private set; }
            public bool ContinuationInvoked { get; private set; }

            public ETaskStatus GetStatus(Guid token)
            {
                ValidateToken(token);
                return _status;
            }

            public void OnCompleted(Action continuation, Guid token)
            {
                ValidateToken(token);
                ContinuationCount++;

                if (_status != ETaskStatus.Pending)
                {
                    InvokeContinuation(continuation);
                    return;
                }

                _continuation = continuation;
            }

            public void SetResult(T result, Guid token)
            {
                ValidateToken(token);
                _result = result;
                _status = ETaskStatus.Succeeded;
                InvokeStoredContinuation();
            }

            public void SetResult(Guid token)
            {
                SetResult(default, token);
            }

            public void SetException(Guid token, Exception exception)
            {
                ValidateToken(token);
                _status = ETaskStatus.Faulted;
                InvokeStoredContinuation();
            }

            public T GetResult(Guid token)
            {
                ValidateToken(token);
                return _result;
            }

            void IETaskAwaiter.GetResult(Guid token)
            {
                GetResult(token);
            }

            private void InvokeStoredContinuation()
            {
                Action continuation = _continuation;
                _continuation = null;
                InvokeContinuation(continuation);
            }

            private void InvokeContinuation(Action continuation)
            {
                ContinuationInvoked = true;
                continuation?.Invoke();
            }

            private void ValidateToken(Guid token)
            {
                if (token != _token)
                {
                    throw new InvalidOperationException("Unexpected ETask token.");
                }
            }
        }
    }
}
