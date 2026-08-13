/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public class EasyFrameworkSettings : ProjectSettingsResources<EasyFrameworkSettings>
    {
        [Header("ResLoader Settings")]
        public int resRequestAliveTime = 60;
    }
}