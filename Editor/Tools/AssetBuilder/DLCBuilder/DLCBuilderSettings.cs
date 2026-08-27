/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    // public interface IDLCBuilderSettingsExtension
    // {
    //     DLCBuilderPackage[] Packages { get; }
    // }
    //
    // class DLCBuilderSettingsExtension : IDLCBuilderSettingsExtension
    // {
    //     public DLCBuilderPackage[] Packages { get; } = new DLCBuilderPackage[]
    //     {
    //         new DLCBuilderPackage(EDLCMode.DLC.ToString(), new string[]
    //         {
    //             "Packages/com.cookie.easyframework/Res/DLC",
    //             EasyFrameworkPreferences.AssetsDataDLCPath
    //         })
    //     };
    // }

    public enum EDLCVersionNameRule
    {
        AppName,
        TooVersion
    }

    [ProjectSettings(ProjectSettingsAttribute.ETag.Editor)]
    public class DLCBuilderSettings : ProjectSettings<DLCBuilderSettings>
    {
        /// <summary>
        /// 最大缓存版本数量，按照时间排序
        /// </summary>
        public int maxCacheNum = 10;
        
        public EDLCVersionNameRule versionNameRule = EDLCVersionNameRule.TooVersion;
        /// <summary>
        /// DLC 打包模式，支持多选
        /// </summary>
        public EDLCMode buildOptions = EDLCMode.List;
        
        // [Header("DLC Packages Per Subdirectory")]
        // public string[] dlcRootDirectories = new []
        // {
        //     "Assets/Res_DLC"
        // };
        
        // public IDLCBuilderSettingsExtension[] Extensions { get; } = EasyFrameworkReflection.CreateInstances<IDLCBuilderSettingsExtension>();
    }
}