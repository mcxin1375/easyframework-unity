using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace EasyFramework.Tests
{
    // Tests may release their isolated manager instances; business code has no destroy entry point.
    internal static class ControllerTestCleanup
    {
        internal static ETask DestroyAsync<T>(this IControllerManager manager) where T : class, IController
            => manager.DestroyAsync(manager.GetController<T>());

        internal static ETask DestroyAsync(this IControllerManager manager, IController controller)
        {
            var cleanup = manager.GetType().GetMethod("DestroyAsync",
                BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IController) }, null);
            if (cleanup == null) throw new MissingMethodException("Internal Controller cleanup was not found.");
            try { return (ETask)cleanup.Invoke(manager, new object[] { controller }); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }
    }
}
