/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Editor
{
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class ToolsCommandSettings : ProjectSettingsEditor<ToolsCommandSettings>
    {
        [Header("Base")] 
        public string dlcVersion;
        public bool hotUpdate;
        public bool developmentBuild;

        [Header("AssetEditor")] 
        public bool resImporter;
        public bool assetImporter;
        public bool assetCreator;
        public bool excelImporter;
        public bool protocImporter;
        
        [Header("AssetBuilder")] 
        public bool assetBundleBuilder;
        public bool dllBuilder;
        public bool dataBuilder;
        
        [Header("DLC")] 
        public bool dlcBuilder;
        public bool dlcReleaseBuilder;

        [Header("Player")] 
        public bool buildPlayer;
        public bool buildProject;
    }
}