/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

#if EF_HYBRIDCLR

using UnityEngine;

namespace EasyFramework.Editor
{
    [ProjectSettings(ProjectSettingsAttribute.ETag.Editor)]
    public class HybridCLRBuilderSettings : ProjectSettings<HybridCLRBuilderSettings>
    {
        [Header("初始化阶段加载所有Dll")]
        public bool loadAll = true;
        [Header("自定义配置需要加载的程序集，其他程序集根据游戏进度动态加载")]
        public string[] customLoadDlls;
        [Header("元数据补充程序集")]
        public string[] stripDlls;
    }
}

#endif