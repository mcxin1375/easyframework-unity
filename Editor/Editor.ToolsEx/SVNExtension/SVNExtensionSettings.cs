/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Editor
{
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class SVNExtensionSettings : ProjectSettingsEditor<SVNExtensionSettings>
    {
        [Header("Active Mode")]
        public bool editorEnabled;
        public bool batchModeEnabled;
        
        public string[] updateDirectories = new[]
        {
            "Assets"
        };
        
        [Header("Commit (ResImporter | AssetImporter | AssetCreator | ExcelImporter | ProtocImporter)")]
        public string[] commitDirectories;
    }
}