/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

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

    [ProjectSettings(ProjectSettingsAttribute.ETag.Editor)]
    public class DLCBuilderSettings : ProjectSettings<DLCBuilderSettings>
    {
        [Header("自定义版本Id，默认使用版本号递增")]
        public string versionCustomId;
        /// <summary>
        /// 最大缓存版本数量，按照时间排序
        /// </summary>
        public int maxCacheNum = 10;
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