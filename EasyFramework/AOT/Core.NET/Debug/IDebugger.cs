
using System;

namespace EasyFramework
{
    public interface IDebugger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception exception);
    }
}
