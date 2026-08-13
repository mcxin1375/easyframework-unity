using System;

namespace EasyFramework
{
    public class NetDebugger : Singleton<NetDebugger>, IDebugger
    {
        public void Log(string message) => Console.WriteLine(message);
        public void LogWarning(string message) => Console.WriteLine(message);
        public void LogError(string message) => Console.WriteLine(message);
        public void LogException(Exception exception) => Console.WriteLine(exception);
    }
}