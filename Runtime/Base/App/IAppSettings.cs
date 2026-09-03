/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public class TestApp1 : IAppSettings
    {
        public string CompanyName { get; }
        public string ProductName { get; }
        public string AppName { get; }
        public string BundleVersion { get; }
        public string BundleIdentifier { get; }
        public int BuildIndex { get; }
        public string CdnURL { get; }
        public string AppVersionURL { get; }
    }
    public class TestApp2 : IAppSettings
    {
        public string CompanyName { get; }
        public string ProductName { get; }
        public string AppName { get; }
        public string BundleVersion { get; }
        public string BundleIdentifier { get; }
        public int BuildIndex { get; }
        public string CdnURL { get; }
        public string AppVersionURL { get; }
    }

    [Reflection(ReflectionMode.Attribute)]
    public partial interface IAppSettings
    {
        string CompanyName { get; }
        string ProductName { get; }
        string AppName { get; }
        string BundleVersion { get; }
        string BundleIdentifier { get; }
        int BuildIndex { get; }
        string CdnURL { get; }
        string AppVersionURL { get; }
    }

    // // 自动生成
    // public partial interface IAppSettings
    // {
    //     static Type[] Types = new Type[]
    //     {
    //         typeof(TestApp1),
    //         typeof(TestApp2),
    //     };
    // }
}
