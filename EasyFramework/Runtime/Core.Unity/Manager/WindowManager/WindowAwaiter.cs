using System;
using System.Threading.Tasks;

namespace EasyFramework
{
    public struct WindowAwaiter<T>
    {
        private static TaskCompletionSource<T> _tcs;
        
        public static bool TrySetResult(in T result)
        {
            if (_tcs != null)
            {
                var tmp = _tcs;
                _tcs = null;
                tmp.SetResult(result);
                return true;
            }
            return false;
        }

        public static Task<T> GetAwaiter()
        {
            if (_tcs != null)
            {
                throw new Exception("同时只能等待一次");
            }
            _tcs = new TaskCompletionSource<T>();
            return _tcs.Task;
        }
    }
}