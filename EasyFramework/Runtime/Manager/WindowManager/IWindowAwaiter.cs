namespace EasyFramework
{
    public interface IWindowAwaiter<T>
    {
        bool TrySetResult(in T result);
        ETask<T> WaitResultAsync();
    }
}