/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    // [EasyProjectSettings("Packages/EasyFrameworkEx/Res_AB/ProjectSettings/EasyFrameworkSettings.asset")]
    public class EasyFrameworkSettings : ProjectSettingsAssetBundle<EasyFrameworkSettings>
    {
        [Header("ResLoader Settings")]
        public int resRequestAliveTime = 60;
    }
}