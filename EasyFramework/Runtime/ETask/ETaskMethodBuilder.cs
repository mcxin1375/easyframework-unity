using System;
using System.Runtime.CompilerServices;

namespace EasyFramework
{
    public readonly struct ETaskMethodBuilder
    {
        private readonly IETaskAwaiter _source;
        private readonly Guid _token;

        public ETaskMethodBuilder(IETaskAwaiter source, Guid token)
        {
            _source = source;
            _token = token;
            // Log.Info($"EasyTaskMethodBuilder: {_token}");
        }

        public static ETaskMethodBuilder Create()
        {
            return new ETaskMethodBuilder(ETask.ETaskStateMachine.Create(out var token), token);
        }

        public void SetResult()
        {
            // Log.Info($"EasyTaskMethodBuilder SetResult: {_token}");
            _source.SetResult(_token);
        }

        public void SetException(Exception exception)
        {
            // Log.Info($"EasyTaskMethodBuilder SetException: {_token}");
            _source.SetException(_token, exception);
        }

        public ETask Task => new(_source, _token);

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            // Log.Info($"EasyTaskMethodBuilder Start: {_token}");
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // Log.Info($"EasyTaskMethodBuilder SetStateMachine: {_token}");
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            // Log.Info($"EasyTaskMethodBuilder AwaitOnCompleted: {_token}");
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            // Log.Info($"EasyTaskMethodBuilder AwaitUnsafeOnCompleted: {_token}");
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }
    
    public readonly struct ETaskMethodBuilder<T>
    {
        private readonly IETaskAwaiter<T> _source;
        private readonly Guid _token;

        public ETaskMethodBuilder(IETaskAwaiter<T> source, Guid token)
        {
            _source = source;
            _token = token;
            // Debug.Log($"EasyTaskMethodBuilder Create: {_source.NameId}");
        }

        public static ETaskMethodBuilder<T> Create()
        {
            return new ETaskMethodBuilder<T>(ETask.ETaskStateMachine<T>.Create(out var token), token);
        }

        public void SetResult(T result)
        {
            // Debug.Log($"EasyTaskMethodBuilder SetResult: {_source.NameId}");
            _source.SetResult(result, _token);
        }

        public void SetException(Exception exception)
        {
            _source.SetException(_token, exception);
            // Log.Error($"[EasyTask Error] {exception.Message}\n{exception.StackTrace}");
        }

        public ETask<T> Task => new(_source, _token);

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            // Debug.Log($"EasyTaskMethodBuilder AwaitOnCompleted: {_source.NameId}");
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            // Debug.Log($"EasyTaskMethodBuilder AwaitUnsafeOnCompleted: {_source.NameId}");
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }
}