/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract class AppBase : IApp
    {
        public abstract string AppVersionFileUrl { get; }
        public abstract string DLCPlatformServerUrl { get; }

        public virtual string[] AppSymbols => null;
    }
}