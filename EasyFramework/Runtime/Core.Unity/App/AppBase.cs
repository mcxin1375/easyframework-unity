/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract class AppBase : IApp
    {
        public abstract string CompanyName { get; }
        public virtual string AppName { get; }
        public virtual string ProductName => AppName;
        public virtual string BundleIdentifier => $"cn.{CompanyName}.{AppName}".ToLower();
        public virtual string BundleVersion => $"{Ver1}.{Ver2}.{Ver3}";
        
        public abstract string AppVersionFileUrl { get; }
        public abstract string DLCPlatformServerUrl { get; }
        public abstract int MainVersion { get; }
        public abstract int Ver1 { get; }
        public abstract int Ver2 { get; }
        public abstract int Ver3 { get; }

        public virtual string[] AppSymbols => null;
        
        protected AppBase()
        {
            AppName = GetType().Name;
        }
    }
}