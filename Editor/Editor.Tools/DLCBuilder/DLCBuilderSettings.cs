/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework.Editor
{
    public interface IDLCBuilderSettingsExtension
    {
        DLCBuilderPackage[] Packages { get; }
    }
    
    class DLCBuilderSettingsExtension : IDLCBuilderSettingsExtension
    {
        public DLCBuilderPackage[] Packages { get; } = new DLCBuilderPackage[]
        {
            new DLCBuilderPackage(EDLCOptions.DLC.ToString(), new string[]
            {
                "Packages/com.cookie.easyframework/Res/DLC",
                EasyFrameworkPreferences.AssetsDataDLCPath
            })
        };
    }

    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class DLCBuilderSettings : ProjectSettingsEditor<DLCBuilderSettings>
    {
        [Header("指定版本名，默认使用ToolVersion版本号")]
        public string dlcVersionId;
        [Header("最大缓存版本数量，按照时间排序")]
        public int maxCacheNum;

        [Header("资源列表模式")]
        public EDLCOptions buildOptions = EDLCOptions.DLC;
        
        [Header("DLC Packages Per Subdirectory")]
        public string[] dlcRootDirectories = new []
        {
            "Assets/Res_DLC"
        };
        
        public IDLCBuilderSettingsExtension[] Extensions { get; } = EasyFrameworkReflection.CreateInstances<IDLCBuilderSettingsExtension>();
    }
}