using System;
using System.Runtime.CompilerServices;

namespace EasyFramework
{
    [AsyncMethodBuilder(typeof(EVoidMethodBuilder))]
    public readonly partial struct EVoid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Forget()
        {
        }
    }

    public static class EVoidScheduler
    {
        public static event Action<Exception> UnobservedTaskException;

        public static bool PropagateOperationCanceledException;

        internal static void PublishUnobservedTaskException(Exception exception)
        {
            if (exception == null) return;
            if (!PropagateOperationCanceledException && exception is OperationCanceledException) return;

            var handler = UnobservedTaskException;
            if (handler != null)
            {
                handler.Invoke(exception);
                return;
            }

            FDebug.LogException(exception);
        }
    }
}
