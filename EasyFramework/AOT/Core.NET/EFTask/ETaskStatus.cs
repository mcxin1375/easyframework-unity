/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public enum ETaskStatus
    {
        /// <summary>The operation has not yet completed.</summary>
        Pending = 0,
        /// <summary>The operation completed successfully.</summary>
        Succeeded = 1,
        /// <summary>The operation completed with an error.</summary>
        Faulted = 2,
        /// <summary>The operation completed due to cancellation.</summary>
        Canceled = 3
    }
    public static class ETaskStatusValue
    {
        public const int Pending = 0;
        public const int Succeeded = 1;
        public const int Faulted = 2;
        public const int Canceled = 3;
    }
}