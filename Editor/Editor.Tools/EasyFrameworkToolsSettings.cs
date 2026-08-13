/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class EasyFrameworkToolsSettings : ProjectSettingsEditor<EasyFrameworkToolsSettings>
    {
        public const int ResImporter = 100;
        public const int AssetImporter = 150;
        public const int AssetCreator = 200;
        public const int ExcelImporter = 250;
        public const int ProtocImporter = 300;
        
        public const int AssetBundleBuilder = 400;
        public const int HybridCLRBuilder = 450;
        public const int DataBuilder = 500;
        
        public const int DLCBuilder = 600;
        public const int DLCReleaseBuilder = 650;
        
        public const int PlayerBuilder = 800;
        
        public const int ToolsCommand = 999;
    }
}