/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using EasyFramework.Editor;
using UnityEngine;

namespace EasyFramework.Server.Editor
{
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class QuickTestExtensionSettings : ProjectSettingsEditor<QuickTestExtensionSettings>
    {
        [Header("HybridCLR")]
        public bool compileDllActiveBuildTarget = true;
        public bool generateAll;
        
        [Header("ToolsBuilder")]
        public bool assetBundleBuilder = true;
        public bool dllBuilder = true;
        public bool dataBuilder = true;
        public bool dlcBuilder = true;
        
        [Header("ServerExtension")]
        public bool uploadDLCApp = true;
        
    }
}